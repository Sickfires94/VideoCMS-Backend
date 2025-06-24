using Azure.Storage.Blobs;
using Backend.Configurations.DataConfigs;
using Backend.Interceptors;
using Backend.Repositories;
using Backend.Repositories.Interface; // Assuming ITagRepository is here, from previous context
using Backend.Repositories.VideoMetadataRepositories;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Backend.Services.Interface;
using Backend.Services.Interfaces; // For IBlobStorageService, etc.
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
        /// <summary>
        /// Adds database context configuration and SQL Server options.
        /// </summary>
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

        /// <summary>
        /// Adds services and repositories related to User management.
        /// </summary>
        public static IServiceCollection AddUserModule(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            return services;
        }

        /// <summary>
        /// Adds services and repositories related to Tag management.
        /// </summary>
        public static IServiceCollection AddTagModule(this IServiceCollection services)
        {
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ITagService, TagService>();
            return services;
        }

        /// <summary>
        /// Adds services and repositories related to Category management.
        /// </summary>
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

        /// <summary>
        /// Configures CORS policy.
        /// </summary>
        /// <param name="policyName">The name of the CORS policy.</param>
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

        /// <summary>
        /// Configures RabbitMQ connection and generic producer.
        /// </summary>
        public static IServiceCollection AddRabbitMqConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqConfig>(configuration.GetSection("backend:RabbitMq"));

            services.AddSingleton<IRabbitMqConnection>(sp =>
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.AutomaticRecoveryEnabled = true;
                factory.TopologyRecoveryEnabled = true;

                var config = sp.GetRequiredService<IOptions<RabbitMqConfig>>().Value;
                // Note: CreateConnectionAsync().GetAwaiter().GetResult() is blocking.
                // For a more robust async setup, consider an IHostedService that manages the connection.
                return new RabbitMqConnection(factory.CreateConnectionAsync().GetAwaiter().GetResult());
            });

            // Registering generic message producer
            services.AddScoped<IMessageProducer, RabbitMqProducerService>();

            return services;
        }

        /// <summary>
        /// Adds configuration for Azure Blob Storage and its service.
        /// </summary>
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

        /// <summary>
        /// Adds configuration for Elasticsearch client.
        /// WARNING: The ServerCertificateValidationCallback bypasses certificate validation. Do NOT use in production.
        /// </summary>
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
                        (sender, certificate, chain, sslPolicyErrors) => true // WARNING: Do NOT use this in production.
                    );

                return new ElasticsearchClient(settings);
            });
            return services;
        }

        /// <summary>
        /// Adds all services, repositories, and hosted services related to video metadata processing and search.
        /// This includes:
        /// - VideoMetadataRepository, IndexVideoMetadataRepository, VideoMetadataSearchingRepository
        /// - VideoMetadataService, IndexVideoMetadataService, VideoMetadataSearchService
        /// - VideoMetadataProducerService (for sending messages about video metadata)
        /// - IndexVideoMetadataConsumerService (for consuming messages to index video metadata)
        /// - RabbitMqTopologyInitializer (for setting up RabbitMQ topology specific to video metadata indexing)
        /// </summary>
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

            // Hosted service for initializing RabbitMQ topology specific to video metadata
            services.AddHostedService<RabbitMqTopologyInitializer>();

            return services;
        }

        /// <summary>
        /// Adds core API services like controllers, API explorer, and Swagger generation.
        /// </summary>
        public static IServiceCollection AddApiCoreServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication and registers the TokenService.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT settings from appsettings.json
            services.Configure<JwtConfig>(configuration.GetSection("backend:Jwt"));

            // Register the Token Service
            services.AddScoped<ITokenService, TokenService>();

            // Configure JWT Bearer Authentication
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
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.key))
                };
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
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
                // If your TagsGenerationConfig.ApiUrl is the FULL URL (e.g., "https://api.example.com/generate"),
                // then HttpClient's BaseAddress should generally NOT be set here,
                // as the service will use the full URL in its GetAsync call.
                // If TagsGenerationConfig.ApiUrl is just the base (e.g., "https://api.example.com/"),
                // then uncomment the client.BaseAddress = baseUri; part.

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(20);
            })
            .ConfigurePrimaryHttpMessageHandler(() => {
                // Optional: If you need to ignore SSL errors for development/testing,
                // DO NOT USE IN PRODUCTION
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });


            return services;
        
        }

        /// <summary>
        /// Adds health check services.
        /// </summary>
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<VideoManagementApplicationContext>(name: "SQL Database Check");
            return services;
        }
    }
}
