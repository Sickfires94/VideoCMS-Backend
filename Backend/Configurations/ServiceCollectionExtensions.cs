using Azure.Storage.Blobs;
using Backend.Configurations.DataConfigs;
using Backend.Interceptors;
using Backend.Repositories;
using Backend.Repositories.Interface;
using Backend.Repositories.VideoMetadataRepositories;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using Backend.Services.Mappers;
using Backend.Services.Mappers.Interfaces;
using Backend.Services.RabbitMq;
using Backend.Services.RabbitMq.Interfaces;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;


namespace Backend.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<VideoMetadataAuditInterceptor>();

            services.AddDbContext<VideoManagementApplicationContext>((serviceProvider, options) => // Use the overload with serviceProvider
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 2,
                            maxRetryDelay: TimeSpan.FromSeconds(1),
                            errorNumbersToAdd: null
                        );
                    });

                // Add the interceptor here
                // Resolve the interceptor from the serviceProvider
                var auditInterceptor = serviceProvider.GetRequiredService<VideoMetadataAuditInterceptor>();
                options.AddInterceptors(auditInterceptor);
            });

            return services;
        }

        public static IServiceCollection AddUserModule(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            return services;
        }

        public static IServiceCollection AddTagModule(this IServiceCollection services)
        {
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ITagService, TagService>();
            return services;
        }

        public static IServiceCollection AddCategoryModule(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            return services;
        }

        public static IServiceCollection AddVideoLogsModule(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadata_changelogRepository, VideoMetadata_changelogRepository>();
            services.AddScoped<IVideoMetadata_changeLogService, VideoMetadata_changeLogService>();
            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName,
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });
            return services;
        }

        public static IServiceCollection AddRabbitMqConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqConfig>(configuration.GetSection("backend:RabbitMq"));

            services.AddSingleton<IRabbitMqConnection>(sp =>
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.AutomaticRecoveryEnabled = true;
                factory.TopologyRecoveryEnabled = true;

                var config = sp.GetRequiredService<IOptions<RabbitMqConfig>>().Value;
                return new RabbitMqConnection(factory.CreateConnectionAsync().GetAwaiter().GetResult());
            });

            // Registering generic message producer
            services.AddScoped<IMessageProducer, RabbitMqProducerService>();

            return services;
        }

        public static IServiceCollection AddAzureBlobStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureStorageConfig>(configuration.GetSection("backend:AzureStorage"));

            services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<IOptions<AzureStorageConfig>>().Value;
                return new BlobServiceClient(config.ConnectionString);
            });

            services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<IOptions<AzureStorageConfig>>().Value;
                var blobServiceClient = x.GetRequiredService<BlobServiceClient>();
                var containerClient = blobServiceClient.GetBlobContainerClient(config.ContainerName);
                containerClient.CreateIfNotExists(); // Ensure the container exists
                return containerClient;
            });

            services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
            return services;
        }

        public static IServiceCollection AddElasticsearchConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ElasticSearchCredentials>(configuration.GetSection("backend:ElasticSearch"));

            services.AddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<ElasticSearchCredentials>>().Value;

                Debug.WriteLine("*****************************");
                Debug.WriteLine("Config: " + config.username);

                var settings = new ElasticsearchClientSettings(new Uri(config.ConnectionURL))
                    .Authentication(new BasicAuthentication(config.username, config.password))
                    .ServerCertificateValidationCallback(
                        (sender, certificate, chain, sslPolicyErrors) => true
                    );

                return new ElasticsearchClient(settings);
            });
            return services;
        }

        public static IServiceCollection AddVideoMetadataModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure options for video metadata indexing
            services.Configure<VideoMetadataIndexingQueueOptions>(configuration.GetSection("VideoMetadataIndexingQueueOptions"));
            services.Configure<VideoMetadataIndexingOptions>(configuration.GetSection("VideoMetadataIndexingOptions"));

            // Repositories
            services.AddScoped<IVideoMetadataRepository, VideoMetadataRepository>();
            services.AddScoped<IIndexVideoMetadataRepository, IndexVideoMetadataRepository>();
            services.AddScoped<IVideoMetadataSearchingRepository, VideoMetadataSearchingRepository>();

            // Services
            services.AddScoped<IVideoMetadataService, VideoMetadataService>();
            services.AddScoped<IIndexVideoMetadataService, IndexVideoMetadataService>();
            services.AddScoped<IVideoMetadataSearchService, VideoMetadataSearchService>();
            services.AddScoped<IVideoMetadataToIndexDtoParser, VideoMetadataToIndexDtoParser>();
            services.AddScoped<IPopulateVideoMetadataService, PopulateVideoMetadataService>();

            // Video metadata producer service
            services.AddSingleton<IVideoMetaDataProducerService>(provider =>
            {
                var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                var settings = provider.GetRequiredService<IOptions<VideoMetadataIndexingQueueOptions>>();
                return new VideoMetadataProducerService(scopeFactory, settings);
            });

            // Consumer service for indexing video metadata
            services.AddHostedService<IndexVideoMetadataConsumerService>(sp =>
            {
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var settings = sp.GetRequiredService<IOptions<VideoMetadataIndexingQueueOptions>>();
                var connection = sp.GetRequiredService<IRabbitMqConnection>();
                return new IndexVideoMetadataConsumerService(connection, scopeFactory, settings);
            });

         
            services.AddHostedService<RabbitMqTopologyInitializer>();

            return services;
        }

        public static IServiceCollection AddApiCoreServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static IServiceCollection AddTokenService(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>(); // Register TokenService
            services.AddScoped<ITokenClaimsAccessor, TokenClaimsAccessor>(); // Register service to access token values (claims)
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtConfig>(configuration.GetSection("backend:Jwt"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtConfig = configuration.GetSection("backend:Jwt").Get<JwtConfig>();
                if (jwtConfig == null)
                {
                    throw new InvalidOperationException("JWT configuration not found in appsettings.json.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,               
                    ValidateAudience = false,             
                    ValidateLifetime = true,             
                    ValidateIssuerSigningKey = true,      
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.key)),
                    ClockSkew = TimeSpan.Zero             
                };
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));

                options.AddPolicy("LoggedIn", policy =>
                {
                    policy.RequireAuthenticatedUser(); // This ensures the user is authenticated
                });
            });
            return services;
        }

      public static IServiceCollection AddTagGenerationService(this IServiceCollection services, IConfiguration configuration)
        {

            var tagsGenerationSection = configuration.GetSection("backend: TagsGenerationConfig");
            var tagsApiUrl = tagsGenerationSection["ApiUrl"];

            Debug.WriteLine($"[DEBUG] TagsGeneration:ApiUrl from configuration: {tagsApiUrl ?? "NULL or NOT FOUND"}");

            services.Configure<TagsGenerationConfig>(configuration.GetSection("backend:TagsGenerationConfig"));

            services.AddHttpClient<IGenerateTagsService, GenerateTagsService>((serviceProvider, client) =>
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(20);
            })
            .ConfigurePrimaryHttpMessageHandler(() => {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });


            return services;
        
        }

       public static IServiceCollection AddMapperServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryMapperService, CategoryMapperService>();
            services.AddScoped<ITagMapperService, TagMapperService>();
            services.AddScoped<IUserMapperService, UserMapperService>();
            services.AddScoped<IVideoMetadataMapperService, VideoMetadataMapperService>();

            return services;
        }
        
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<VideoManagementApplicationContext>(name: "SQL Database Check");
            return services;
        }
    }
}
