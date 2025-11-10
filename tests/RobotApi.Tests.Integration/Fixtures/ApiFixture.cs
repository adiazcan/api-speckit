using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RobotApi.Tests.Integration.Fixtures;

/// <summary>
/// Test fixture for integration tests using WebApplicationFactory.
/// Provides a test server with in-memory data stores.
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure test-specific services if needed
            // For now, the default configuration works fine
        });

        builder.UseEnvironment("Development");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    /// <summary>
    /// Creates an HTTP client configured for testing.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string token = "admin-token")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }
}
