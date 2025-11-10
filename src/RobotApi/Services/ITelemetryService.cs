using RobotApi.Models;
using RobotApi.Models.Dtos;

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

    /// <summary>
    /// Gets historical telemetry data with time range filtering, pagination, and optional field selection.
    /// </summary>
    /// <param name="robotId">The robot ID to retrieve telemetry for.</param>
    /// <param name="request">The query parameters for historical telemetry.</param>
    /// <returns>Paginated response containing telemetry snapshots and metadata.</returns>
    Task<TelemetryHistoryResponse> GetHistoricalTelemetryAsync(string robotId, TelemetryHistoryRequest request);
}
