using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory data store for Telemetry entities using thread-safe ConcurrentDictionary.
/// Maintains both latest telemetry per robot and historical snapshots.
/// </summary>
public sealed class TelemetryStore : IDataStore<Telemetry>
{
    private readonly ConcurrentDictionary<string, Telemetry> _telemetry = new();
    private readonly ConcurrentDictionary<string, Telemetry> _latestByRobot = new();
    private readonly ILogger<TelemetryStore> _logger;

    public TelemetryStore(ILogger<TelemetryStore> logger)
    {
        _logger = logger;
    }

    public Task<Telemetry?> GetByIdAsync(string id)
    {
        _telemetry.TryGetValue(id, out var telemetry);
        return Task.FromResult(telemetry);
    }

    public Task<IEnumerable<Telemetry>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Telemetry>>(_telemetry.Values.ToList());
    }

    public Task<Telemetry> AddAsync(Telemetry entity)
    {
        if (_telemetry.TryAdd(entity.Id, entity))
        {
            // Update latest telemetry for this robot
            _latestByRobot.AddOrUpdate(entity.RobotId, entity, (key, existing) =>
            {
                // Only update if this telemetry is newer
                return entity.Timestamp > existing.Timestamp ? entity : existing;
            });

            _logger.LogDebug("Added telemetry {TelemetryId} for robot {RobotId}", entity.Id, entity.RobotId);
            return Task.FromResult(entity);
        }

        throw new InvalidOperationException($"Telemetry with ID '{entity.Id}' already exists");
    }

    public Task<Telemetry?> UpdateAsync(string id, Telemetry entity)
    {
        if (_telemetry.ContainsKey(id))
        {
            _telemetry[id] = entity;
            _logger.LogDebug("Updated telemetry {TelemetryId}", id);
            return Task.FromResult<Telemetry?>(entity);
        }

        throw new KeyNotFoundException($"Telemetry with ID '{id}' not found");
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _telemetry.TryRemove(id, out _);
        if (removed)
        {
            _logger.LogDebug("Deleted telemetry {TelemetryId}", id);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_telemetry.ContainsKey(id));
    }

    /// <summary>
    /// Gets the most recent telemetry snapshot for a specific robot.
    /// </summary>
    public Task<Telemetry?> GetLatestByRobotIdAsync(string robotId)
    {
        _latestByRobot.TryGetValue(robotId, out var telemetry);
        return Task.FromResult(telemetry);
    }

    /// <summary>
    /// Gets all telemetry snapshots for a specific robot, ordered by timestamp descending.
    /// </summary>
    public Task<IEnumerable<Telemetry>> GetByRobotIdAsync(string robotId)
    {
        var telemetryList = _telemetry.Values
            .Where(t => t.RobotId == robotId)
            .OrderByDescending(t => t.Timestamp)
            .ToList();

        return Task.FromResult<IEnumerable<Telemetry>>(telemetryList);
    }

    /// <summary>
    /// Gets telemetry snapshots for a robot within a time range.
    /// </summary>
    public Task<IEnumerable<Telemetry>> GetByRobotIdAndTimeRangeAsync(
        string robotId,
        DateTime startTime,
        DateTime endTime)
    {
        var telemetryList = _telemetry.Values
            .Where(t => t.RobotId == robotId &&
                       t.Timestamp >= startTime &&
                       t.Timestamp <= endTime)
            .OrderBy(t => t.Timestamp)
            .ToList();

        return Task.FromResult<IEnumerable<Telemetry>>(telemetryList);
    }
}
