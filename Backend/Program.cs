using Backend.Configurations;
using Backend.Configurations.DataConfigs;
using Elastic.Apm.SerilogEnricher;
using Elastic.Channels;
using Elastic.Clients.Elasticsearch;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using DataStreamName = Elastic.Ingest.Elasticsearch.DataStreams.DataStreamName;

var builder = WebApplication.CreateBuilder(args);

const string allowAllOrigins = "AllowAll";

// ─────────────────────────────────────────────────────────────
// 🟡 Bootstrap logger (minimal logger before Serilog is configured)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();


 // Setup Serilog with Elastic Search Sink
try
{
    Log.Information("Starting web host");

    builder.Host.UseSerilog((context, services, config) =>
    {
        var elasticConfig = context.Configuration
            .GetSection("backend:ElasticSearch")
            .Get<ElasticSearchCredentials>();

        var connectionSettings = new ElasticsearchClientSettings(new Uri(elasticConfig.ConnectionURL))
            .Authentication(new BasicAuthentication(elasticConfig.username, elasticConfig.password))
            .ServerCertificateValidationCallback((_, _, _, _) => true); // Don't use in production

        var client = new Elastic.Clients.Elasticsearch.ElasticsearchClient(connectionSettings);

        
        {

            int LogConnectionRetries = 0;
            while (LogConnectionRetries < 5)
            {
                try
                {
                    config
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .Enrich.WithElasticApmCorrelationInfo()
                        .WriteTo.Console(new EcsTextFormatter())
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(client.Transport)
                        {
                            DataStream = new DataStreamName("logs", "videocms"),
                            BootstrapMethod = BootstrapMethod.Failure,
                            TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>
                            {
                                MapCustom = (e, _) => e
                            },
                            ConfigureChannel = channelOpts =>
                            {
                                channelOpts.BufferOptions = new BufferOptions
                                {
                                    ExportMaxConcurrency = 4
                                };
                            }
                        });
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "Failed to configure Elasticsearch sink, Connection Attempt: " + LogConnectionRetries);
                    // Thread.Sleep(5000);
                    LogConnectionRetries++;
                }
            }
        }
        
    });

    Debug.WriteLine("Serilog setup Successfully");

    Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

    builder.Logging.ClearProviders(); // Let Serilog handle logging
    builder.Logging.AddConsole();     // Optional: also add console for app logs

    builder.Services
        .AddDatabaseConfiguration(builder.Configuration)
        .AddCorsConfiguration(allowAllOrigins)
        .AddRabbitMqConfiguration(builder.Configuration)
        .AddAzureBlobStorageConfiguration(builder.Configuration)
        .AddElasticsearchConfiguration(builder.Configuration)
        .AddTagGenerationService(builder.Configuration)
        .AddMapperServices()
        .AddUserModule()
        .AddTokenService()
        .AddTagModule() 
        .AddCategoryModule()
        .AddVideoLogsModule()
        .AddVideoMetadataModule(builder.Configuration)
        .AddApiCoreServices()
        .AddJwtAuthentication(builder.Configuration)
        .AddAuthorizationConfiguration()
        .AddHealthCheckServices();

    var app = builder.Build();


    // ─────────────────────────────────────────────────────────────
    app.UseSwaggerDocumentation(app.Environment);
    app.UseRoutingMiddleware();
    app.UseCorsPolicy(allowAllOrigins);
    app.UseSecurityMiddlewares();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
