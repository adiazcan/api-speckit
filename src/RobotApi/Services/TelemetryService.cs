using RobotApi.Data;
using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Service for telemetry data management and retrieval.
/// </summary>
public sealed class TelemetryService : ITelemetryService
{
    private readonly TelemetryStore _telemetryStore;
    private readonly RobotStore _robotStore;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(
        TelemetryStore telemetryStore,
        RobotStore robotStore,
        ILogger<TelemetryService> logger)
    {
        _telemetryStore = telemetryStore;
        _robotStore = robotStore;
        _logger = logger;
    }

    public async Task<Telemetry?> GetCurrentTelemetryAsync(string robotId)
    {
        // Validate robot exists
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            _logger.LogWarning("Telemetry requested for non-existent robot {RobotId}", robotId);
            throw new KeyNotFoundException($"Robot with ID '{robotId}' not found");
        }

        // Get latest telemetry
        var telemetry = await _telemetryStore.GetLatestByRobotIdAsync(robotId);
        
        if (telemetry == null)
        {
            _logger.LogInformation("No telemetry available for robot {RobotId}", robotId);
            return null;
        }

        var freshness = telemetry.GetDataFreshness();
        
        if (freshness > 60)
        {
            _logger.LogWarning(
                "Stale telemetry data for robot {RobotId}: {Freshness:F1} seconds old",
                robotId, freshness);
        }
        else
        {
            _logger.LogInformation(
                "Retrieved current telemetry for robot {RobotId} (freshness: {Freshness:F1}s)",
                robotId, freshness);
        }

        return telemetry;
    }

    public async Task<IEnumerable<Telemetry>> GetTelemetryHistoryAsync(string robotId)
    {
        // Validate robot exists
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            throw new KeyNotFoundException($"Robot with ID '{robotId}' not found");
        }

        var history = await _telemetryStore.GetByRobotIdAsync(robotId);
        
        _logger.LogInformation(
            "Retrieved {Count} telemetry snapshots for robot {RobotId}",
            history.Count(), robotId);

        return history;
    }
}
