using Azure.Storage.Blobs;
using Backend.Configurations;
using Backend.DTOs;
using Backend.Services;
using Backend.Services.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policy name (can be moved to appsettings.json if preferred)
const string allowAllOrigins = "AllowAll";

// --- Service Configuration ---
builder.Services
    .AddDatabaseConfiguration(builder.Configuration)
    .AddCorsConfiguration(allowAllOrigins)
    .AddRabbitMqConfiguration(builder.Configuration)
    .AddAzureBlobStorageConfiguration(builder.Configuration)
    .AddElasticsearchConfiguration(builder.Configuration)
    .AddGenericPublisher()
    .AddVideoMetadataProducerService(builder.Configuration)
    .AddAutoMapper(typeof(Program))
    .AddVideoMetadataRepository()
    .AddVideoMetadataService()
    .AddApplicationServices() // Contains UserService
    .AddHealthCheckServices()
    .AddApiCoreServices() // Controllers, Swagger, Endpoints
    .AddAuthenticationConfiguration(); // JWT Bearer

var app = builder.Build();

// --- Middleware Pipeline Configuration ---
app.UseSwaggerDocumentation(app.Environment);
app.UseRoutingMiddleware(); // Now only handles the UseRouting middleware
app.UseCorsPolicy(allowAllOrigins);
app.UseSecurityMiddlewares(); // HttpsRedirection, Authentication, Authorization

// --- Endpoint Mapping ---
// These calls must be made on the 'app' instance (WebApplication),
// which implicitly provides IEndpointRouteBuilder capabilities after UseRouting()
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
