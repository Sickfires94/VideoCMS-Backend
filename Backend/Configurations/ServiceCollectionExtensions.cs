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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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

            services.AddScoped<IMessageProducer, RabbitMqPublisherService>();

            return services;
        }


        public static IServiceCollection AddVideoMetadataRepository(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataRepository, VideoMetadataRepository>();
            return services;
        }

        public static IServiceCollection AddGenericPublisher(this IServiceCollection services)
        {
            services.AddScoped<IMessageProducer, RabbitMqPublisherService>();
            return services;
        }

        public static IServiceCollection AddVideoMetadataService(this IServiceCollection services)
        {
            services.AddScoped<IVideoMetadataService, VideoMetadataService>();
            return services;
        }

        public static IServiceCollection AddVideoMetadataProducerService(this IServiceCollection services)
        {
            services.AddSingleton<IVideoMetaDataProducer, VideoMetadataProducerService>();
            return services;
        }

        public static IServiceCollection AddVieoMetadataPublisherService(this IServiceCollection services, IConfiguration configuration)
        {


            services.AddSingleton<IVideoMetaDataProducer>(sp =>
            {
                var videoProducerServiceConfig = sp.GetRequiredService<IOptions<VideoMetadataProducerConfig>>().Value;
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();


                return new VideoMetadataProducerService(
                    scopeFactory,
                    videoProducerServiceConfig.ExchangeName,
                    videoProducerServiceConfig.RoutingKey,
                    videoProducerServiceConfig.EntityType
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

        public static IServiceCollection AddElasticsearchConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ElasticSearchCredentials>(configuration.GetSection("backend:ElasticSearch"));

            services.AddSingleton<ElasticsearchClient>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<ElasticSearchCredentials>>().Value;

                var settings = new ElasticsearchClientSettings(new Uri(config.ConnectionURL))
                    .Authentication(new BasicAuthentication(config.username, config.password))
                    .ServerCertificateValidationCallback(
                        (sender, certificate, chain, sslPolicyErrors) => true // WARNING: Do NOT use this in production.
                    );

                return new ElasticsearchClient(settings);
            });
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
