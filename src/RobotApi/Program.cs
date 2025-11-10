using System.Text.Json.Serialization;
using RobotApi.Data;
using RobotApi.Endpoints;
using RobotApi.Extensions;
using RobotApi.Middleware;
using RobotApi.Models;
using RobotApi.Services;
using Serilog;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting IoT Robot Control & Telemetry API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddEndpointsApiExplorer();

    // Configure JSON options for enum serialization
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
    });

    // Add FluentValidation
    builder.Services.AddFluentValidators();

    // Add CORS
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (corsOrigins.Contains("*"))
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // Register data stores as singletons (in-memory)
    builder.Services.AddSingleton<OperatorStore>();
    builder.Services.AddSingleton<RobotStore>();
    builder.Services.AddSingleton<CommandStore>();
    builder.Services.AddSingleton<TelemetryStore>();
    builder.Services.AddSingleton<EventStore>();
    builder.Services.AddSingleton<SubscriptionStore>();
    builder.Services.AddSingleton<DataSeeder>();

    // Register data store interfaces
    builder.Services.AddSingleton<IDataStore<Operator>>(sp => sp.GetRequiredService<OperatorStore>());
    builder.Services.AddSingleton<IDataStore<Robot>>(sp => sp.GetRequiredService<RobotStore>());
    builder.Services.AddSingleton<IDataStore<Command>>(sp => sp.GetRequiredService<CommandStore>());
    builder.Services.AddSingleton<IDataStore<Telemetry>>(sp => sp.GetRequiredService<TelemetryStore>());
    builder.Services.AddSingleton<IDataStore<Event>>(sp => sp.GetRequiredService<EventStore>());
    builder.Services.AddSingleton<IDataStore<Subscription>>(sp => sp.GetRequiredService<SubscriptionStore>());
    
    // Register specialized store interfaces
    builder.Services.AddSingleton<IRobotStore>(sp => sp.GetRequiredService<RobotStore>());
    builder.Services.AddSingleton<ICommandStore>(sp => sp.GetRequiredService<CommandStore>());

    // Register services
    builder.Services.AddSingleton<ICommandService, CommandService>();
    builder.Services.AddSingleton<ITelemetryService, TelemetryService>();
    builder.Services.AddSingleton<ITelemetryExportService, TelemetryExportService>();
    builder.Services.AddSingleton<IEventService, EventService>();
    builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
    builder.Services.AddSingleton<IRobotService, RobotService>();
    
    // Register HTTP client for webhook delivery
    builder.Services.AddHttpClient<IWebhookDeliveryService, WebhookDeliveryService>();

    // Register background services
    builder.Services.AddHostedService<CommandExecutorService>();
    builder.Services.AddHostedService<TelemetryGeneratorService>();
    builder.Services.AddHostedService<WebhookProcessorService>();

    var app = builder.Build();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    // Add global exception handler
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    // Add request logging middleware
    app.UseMiddleware<RequestLoggingMiddleware>();

    // Add Serilog request logging
    app.UseSerilogRequestLogging();

    // Add CORS
    app.UseCors();

    // Add mock authentication
    app.UseMiddleware<MockAuthenticationMiddleware>();

    // Add rate limiting middleware (after authentication)
    app.UseMiddleware<RateLimitingMiddleware>();

    // Configure the HTTP request pipeline
    app.MapGet("/", () => new
    {
        message = "IoT Robot Control & Telemetry API",
        version = "1.0.0",
        status = "operational"
    }).WithName("Root").WithTags("Status");

    // Map API endpoints
    app.MapCommandEndpoints();
    app.MapTelemetryEndpoints();
    
    var v1 = app.MapGroup("/v1");
    v1.MapGroup("/subscriptions").MapSubscriptionEndpoints().WithTags("Subscriptions");
    v1.MapGroup("/events").MapEventEndpoints().WithTags("Events");
    v1.MapGroup("/robots").MapRobotEndpoints().WithTags("Robots");
    v1.MapGroup("/health").MapHealthEndpoints().WithTags("Health");
    v1.MapDocumentationEndpoints().WithTags("Documentation");

    Log.Information("IoT Robot Control & Telemetry API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
