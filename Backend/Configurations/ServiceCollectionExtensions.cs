using Azure.Storage.Blobs;
using Backend.Configurations.DataConfigs;
using Backend.Repositories.VideoMetadataRepositories;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Services.RabbitMq;
using Backend.Services.RabbitMq.Interfaces;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics;

namespace Backend.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VideoManagementApplicationContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 2,
                            maxRetryDelay: TimeSpan.FromSeconds(1),
                            errorNumbersToAdd: null
                        );
                    }));
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

            services.AddScoped<IMessageProducer, RabbitMqProducerService>();

            return services;
        }

        public static IServiceCollection InitializeRabbitMqTopology(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VideoMetadataIndexingOptions>(configuration.GetSection("VideoMetadataIndexingOptions"));
            services.AddHostedService<RabbitMqTopologyInitializer>();
            return services;
        }

        public static IServiceCollection AddIndexVideoMetadataConsumerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VideoMetadataIndexingOptions>(configuration.GetSection("VideoMetadataIndexingOptions"));
            services.AddHostedService<IndexVideoMetadataConsumerService>(sp =>
            {

                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var settings = sp.GetRequiredService<IOptions<VideoMetadataIndexingOptions>>();
                var connection = sp.GetRequiredService<IRabbitMqConnection>();

                return new IndexVideoMetadataConsumerService(connection, scopeFactory, settings);

            });
            return services;
        }


        public static IServiceCollection AddVideoMetadataRepository(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataRepository, VideoMetadataRepository>();
            return services;
        }

        public static IServiceCollection AddGenericProducer(this IServiceCollection services)
        {
            services.AddScoped<IMessageProducer, RabbitMqProducerService>();
            return services;
        }

        public static IServiceCollection AddVideoMetadataService(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataService, VideoMetadataService>();
            return services;
        }

        public static IServiceCollection AddVideoMetadataProducerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VideoMetadataIndexingOptions>(configuration.GetSection("VideoMetadataIndexingOptions"));

            services.AddSingleton<IVideoMetaDataProducerService>(provider =>
            {
                var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                var logger = provider.GetRequiredService<ILogger<VideoMetadataProducerService>>();
                var settings = provider.GetRequiredService<IOptions<VideoMetadataIndexingOptions>>();

                return new VideoMetadataProducerService(
                    scopeFactory,
                    settings
                );
            });

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

        public static IServiceCollection AddIndexVideoMetadataService(this IServiceCollection services)
        {
            services.AddScoped<IIndexVideoMetadataService, IndexVideoMetadataService>();
            return services;
        }

        public static IServiceCollection AddIndexVideoMetadataRepository(this IServiceCollection services)
        {
            services.AddScoped<IIndexVideoMetadataRepository, IndexVideoMetadataRepository>();
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
                        (sender, certificate, chain, sslPolicyErrors) => true // WARNING: Do NOT use this in production.
                    );

                return new ElasticsearchClient(settings);
            });
            return services;
        }


        public static IServiceCollection AddVideoMetadataSearchService(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataSearchService, VideoMetadataSearchService>();
            return services;
        }


        public static IServiceCollection AddVideoMetadataSearchRepository(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataSearchingRepository, VideoMetadataSearchingRepository>();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            return services;
        }

        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<VideoManagementApplicationContext>(name: "SQL Database Check");
            return services;
        }

        public static IServiceCollection AddApiCoreServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
        {
            services.AddAuthentication("Bearer").AddJwtBearer();
            return services;
        }
    }
}
