using System.Text;
using RobotApi.Data;

namespace RobotApi.Services;

/// <summary>
/// Service for exporting telemetry data to CSV format.
/// </summary>
public sealed class TelemetryExportService : ITelemetryExportService
{
    private readonly TelemetryStore _telemetryStore;
    private readonly RobotStore _robotStore;
    private readonly ILogger<TelemetryExportService> _logger;

    public TelemetryExportService(
        TelemetryStore telemetryStore,
        RobotStore robotStore,
        ILogger<TelemetryExportService> logger)
    {
        _telemetryStore = telemetryStore;
        _robotStore = robotStore;
        _logger = logger;
    }

    public async Task<string> ExportToCsvAsync(string robotId, DateTime startTime, DateTime endTime, string[]? telemetryTypes = null)
    {
        // Validate robot exists
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            throw new KeyNotFoundException($"Robot with ID '{robotId}' not found");
        }

        // Retrieve telemetry data
        var telemetryData = await _telemetryStore.GetByRobotIdAndTimeRangeAsync(robotId, startTime, endTime);
        var orderedData = telemetryData.OrderBy(t => t.Timestamp).ToList();

        // Determine which fields to include
        var includeAllFields = telemetryTypes == null || telemetryTypes.Length == 0;
        HashSet<string>? fieldSet = null;
        if (!includeAllFields && telemetryTypes != null)
        {
            fieldSet = new HashSet<string>(telemetryTypes, StringComparer.OrdinalIgnoreCase);
        }

        var csv = new StringBuilder();

        // Build header row
        var headers = new List<string> { "Timestamp", "RobotId" };
        
        if (includeAllFields || fieldSet!.Contains("speed"))
            headers.Add("Speed");
        if (includeAllFields || fieldSet!.Contains("batteryLevel"))
            headers.Add("BatteryLevel");
        if (includeAllFields || fieldSet!.Contains("temperature"))
            headers.Add("Temperature");
        if (includeAllFields || fieldSet!.Contains("position"))
        {
            headers.Add("Position.Type");
            headers.Add("Position.Latitude");
            headers.Add("Position.Longitude");
            headers.Add("Position.Altitude");
            headers.Add("Position.X");
            headers.Add("Position.Y");
            headers.Add("Position.Z");
        }
        if (includeAllFields || fieldSet!.Contains("orientation"))
        {
            headers.Add("Orientation.Pitch");
            headers.Add("Orientation.Roll");
            headers.Add("Orientation.Yaw");
        }

        csv.AppendLine(string.Join(",", headers));

        // Build data rows
        foreach (var telemetry in orderedData)
        {
            var values = new List<string>
            {
                EscapeCsvValue(telemetry.Timestamp.ToString("O")),
                EscapeCsvValue(telemetry.RobotId)
            };

            if (includeAllFields || fieldSet!.Contains("speed"))
                values.Add(telemetry.Speed.ToString("F2"));
            if (includeAllFields || fieldSet!.Contains("batteryLevel"))
                values.Add(telemetry.BatteryLevel.ToString());
            if (includeAllFields || fieldSet!.Contains("temperature"))
                values.Add(telemetry.Temperature.ToString("F2"));
            if (includeAllFields || fieldSet!.Contains("position"))
            {
                values.Add(EscapeCsvValue(telemetry.Position.Type));
                values.Add(telemetry.Position.Latitude?.ToString("F6") ?? "");
                values.Add(telemetry.Position.Longitude?.ToString("F6") ?? "");
                values.Add(telemetry.Position.Altitude?.ToString("F2") ?? "");
                values.Add(telemetry.Position.X?.ToString("F2") ?? "");
                values.Add(telemetry.Position.Y?.ToString("F2") ?? "");
                values.Add(telemetry.Position.Z?.ToString("F2") ?? "");
            }
            if (includeAllFields || fieldSet!.Contains("orientation"))
            {
                values.Add(telemetry.Orientation.Pitch.ToString("F2"));
                values.Add(telemetry.Orientation.Roll.ToString("F2"));
                values.Add(telemetry.Orientation.Yaw.ToString("F2"));
            }

            csv.AppendLine(string.Join(",", values));
        }

        var csvString = csv.ToString();
        var csvSizeKb = Encoding.UTF8.GetByteCount(csvString) / 1024.0;

        _logger.LogInformation(
            "Exported telemetry to CSV for robot {RobotId}: {RecordCount} records, {SizeKb:F2} KB, time range {StartTime} to {EndTime}",
            robotId, orderedData.Count, csvSizeKb, startTime, endTime);

        return csvString;
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Escape values containing comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
