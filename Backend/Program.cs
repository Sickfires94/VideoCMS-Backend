using Azure.Storage.Blobs;
using Backend.Configurations;
using Backend.DTOs; // Not directly used here, but good to keep if DTOs are referenced indirectly by other configs.
using Backend.Services; // Not directly used here.
using Backend.Services.Interfaces; // Not directly used here.
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policy name
const string allowAllOrigins = "AllowAll";

builder.Logging.AddConsole();

// --- Service Configuration ---
// Infrastructure Services (typically configured first)
builder.Services
    .AddDatabaseConfiguration(builder.Configuration)
    .AddCorsConfiguration(allowAllOrigins)
    .AddRabbitMqConfiguration(builder.Configuration) // Includes generic IMessageProducer
    .AddAzureBlobStorageConfiguration(builder.Configuration)
    .AddElasticsearchConfiguration(builder.Configuration);

// Domain/Feature Modules (contain repositories and services specific to a domain)
builder.Services
    .AddUserModule()
    .AddTagModule()
    .AddCategoryModule()
    .AddVideoMetadataModule(builder.Configuration); // Includes all video metadata related services and hosted services

// Core API and Cross-Cutting Concerns
builder.Services
    .AddApiCoreServices() // Controllers, Swagger, Endpoints
    .AddJwtAuthentication(builder.Configuration) // JWT Bearer
    .AddHealthCheckServices(); // Health Checks (can be placed after other services are defined)

var app = builder.Build();

// --- Middleware Pipeline Configuration ---
// Order of middleware matters!

// Development specific middleware (e.g., Swagger)
app.UseSwaggerDocumentation(app.Environment);

// Routing, CORS, and Security (generally ordered this way)
app.UseRoutingMiddleware(); // Now only handles the UseRouting middleware
app.UseCorsPolicy(allowAllOrigins); // Must be after UseRouting and before UseAuthentication/UseAuthorization
app.UseSecurityMiddlewares(); // HttpsRedirection, Authentication, Authorization

// --- Endpoint Mapping ---
// Map controllers and other endpoints
app.MapControllers();
app.MapHealthChecks("/health"); // Expose health check endpoint

app.Run();
