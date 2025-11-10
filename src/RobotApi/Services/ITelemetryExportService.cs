namespace RobotApi.Services;

/// <summary>
/// Service interface for exporting telemetry data to various formats.
/// </summary>
public interface ITelemetryExportService
{
    /// <summary>
    /// Exports telemetry data to CSV format.
    /// </summary>
    /// <param name="robotId">The robot ID to export telemetry for.</param>
    /// <param name="startTime">Start of the time range (inclusive) in UTC.</param>
    /// <param name="endTime">End of the time range (inclusive) in UTC.</param>
    /// <param name="telemetryTypes">Optional field names to include. If null or empty, all fields are included.</param>
    /// <returns>CSV string with headers and data rows.</returns>
    Task<string> ExportToCsvAsync(string robotId, DateTime startTime, DateTime endTime, string[]? telemetryTypes = null);
}
