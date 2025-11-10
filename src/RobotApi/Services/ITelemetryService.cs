using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Service interface for telemetry data management and retrieval.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Gets the most recent telemetry snapshot for a robot.
    /// </summary>
    /// <param name="robotId">The robot ID to retrieve telemetry for.</param>
    /// <returns>The latest telemetry snapshot if available, otherwise null.</returns>
    Task<Telemetry?> GetCurrentTelemetryAsync(string robotId);

    /// <summary>
    /// Gets all telemetry snapshots for a robot.
    /// </summary>
    /// <param name="robotId">The robot ID to retrieve telemetry for.</param>
    /// <returns>List of telemetry snapshots ordered by timestamp descending.</returns>
    Task<IEnumerable<Telemetry>> GetTelemetryHistoryAsync(string robotId);
}
