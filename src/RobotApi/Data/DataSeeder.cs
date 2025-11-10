using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// Service responsible for seeding initial data into all data stores.
/// </summary>
public class DataSeeder
{
    private readonly ILogger<DataSeeder> _logger;
    private readonly OperatorStore _operatorStore;
    private readonly RobotStore _robotStore;

    public DataSeeder(
        ILogger<DataSeeder> logger,
        OperatorStore operatorStore,
        RobotStore robotStore)
    {
        _logger = logger;
        _operatorStore = operatorStore;
        _robotStore = robotStore;
    }

    /// <summary>
    /// Seeds all data stores with initial test data.
    /// Called during application startup.
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Data seeding completed - stores initialized with test data");

        // Log summary
        var operators = await _operatorStore.GetAllAsync();
        var robots = await _robotStore.GetAllAsync();

        _logger.LogInformation("Available operators: {Count}", operators.Count());
        _logger.LogInformation("Available robots: {Count}", robots.Count());
    }
}
