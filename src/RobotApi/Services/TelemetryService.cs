using System.Diagnostics;
using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;

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

    public async Task<TelemetryHistoryResponse> GetHistoricalTelemetryAsync(string robotId, TelemetryHistoryRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        // Validate robot exists
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            throw new KeyNotFoundException($"Robot with ID '{robotId}' not found");
        }

        // Retrieve telemetry within time range
        var telemetryData = await _telemetryStore.GetByRobotIdAndTimeRangeAsync(
            robotId, request.StartTime, request.EndTime);

        var totalCount = telemetryData.Count();

        // Apply pagination
        var paginatedData = telemetryData
            .OrderByDescending(t => t.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Convert to response DTOs (field projection would be applied in DTO mapping if needed)
        var responseData = paginatedData
            .Select(TelemetryResponse.FromTelemetry)
            .ToArray();

        stopwatch.Stop();

        _logger.LogInformation(
            "Historical telemetry query for robot {RobotId}: {RecordCount} records found in range {StartTime} to {EndTime}, returned page {Page} ({PageRecords} records) in {ElapsedMs}ms",
            robotId, totalCount, request.StartTime, request.EndTime, request.Page, responseData.Length, stopwatch.ElapsedMilliseconds);

        return new TelemetryHistoryResponse
        {
            Data = responseData,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
