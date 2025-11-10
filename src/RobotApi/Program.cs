using RobotApi.Data;
using RobotApi.Extensions;
using RobotApi.Middleware;
using RobotApi.Models;
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
    builder.Services.AddSingleton<DataSeeder>();

    // Register data store interfaces
    builder.Services.AddSingleton<IDataStore<Operator>>(sp => sp.GetRequiredService<OperatorStore>());
    builder.Services.AddSingleton<IDataStore<Robot>>(sp => sp.GetRequiredService<RobotStore>());

    var app = builder.Build();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    // Add global exception handler
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    // Add Serilog request logging
    app.UseSerilogRequestLogging();

    // Add CORS
    app.UseCors();

    // Add mock authentication
    app.UseMiddleware<MockAuthenticationMiddleware>();

    // Configure the HTTP request pipeline
    app.MapGet("/", () => new
    {
        message = "IoT Robot Control & Telemetry API",
        version = "1.0.0",
        status = "operational"
    }).WithName("Root").WithTags("Status");

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
