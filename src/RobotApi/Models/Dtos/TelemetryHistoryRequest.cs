namespace RobotApi.Models.Dtos;

/// <summary>
/// Request model for querying historical telemetry data with time range filtering, field selection, and pagination.
/// </summary>
public class TelemetryHistoryRequest
{
    /// <summary>
    /// Start of the time range (inclusive) in UTC.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End of the time range (inclusive) in UTC.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Optional array of telemetry field names to include in the response (e.g., "speed", "batteryLevel", "temperature").
    /// If null or empty, all fields are included.
    /// </summary>
    public string[]? TelemetryTypes { get; set; }

    /// <summary>
    /// Page number for pagination (1-based). Default is 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of records per page. Default is 100, maximum is 1000.
    /// </summary>
    public int PageSize { get; set; } = 100;
}
