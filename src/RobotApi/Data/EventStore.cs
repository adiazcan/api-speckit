using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory store for events with time-based filtering.
/// </summary>
public sealed class EventStore : IDataStore<Event>
{
    private readonly ConcurrentBag<Event> _events = new();
    private readonly ILogger<EventStore> _logger;

    public EventStore(ILogger<EventStore> logger)
    {
        _logger = logger;
    }

    public Task<Event?> GetByIdAsync(string id)
    {
        var evt = _events.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(evt);
    }

    public Task<IEnumerable<Event>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Event>>(_events.OrderByDescending(e => e.Timestamp).ToList());
    }

    public Task<Event> AddAsync(Event entity)
    {
        _events.Add(entity);
        _logger.LogDebug("Added event {EventId} of type {EventType} for robot {RobotId}", 
            entity.Id, entity.EventType, entity.RobotId);
        return Task.FromResult(entity);
    }

    public Task<Event?> UpdateAsync(string id, Event entity)
    {
        // Events are immutable, no update needed
        return Task.FromResult<Event?>(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        // Events are not deleted
        return Task.FromResult(false);
    }

    public Task<bool> ExistsAsync(string id)
    {
        var exists = _events.Any(e => e.Id == id);
        return Task.FromResult(exists);
    }

    /// <summary>
    /// Gets events by robot ID.
    /// </summary>
    public Task<IEnumerable<Event>> GetByRobotIdAsync(string robotId)
    {
        var events = _events
            .Where(e => e.RobotId == robotId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult<IEnumerable<Event>>(events);
    }

    /// <summary>
    /// Gets events within a time range.
    /// </summary>
    public Task<IEnumerable<Event>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        var events = _events
            .Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult<IEnumerable<Event>>(events);
    }

    /// <summary>
    /// Gets events by type.
    /// </summary>
    public Task<IEnumerable<Event>> GetByEventTypeAsync(string eventType)
    {
        var events = _events
            .Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult<IEnumerable<Event>>(events);
    }

    /// <summary>
    /// Gets events by severity.
    /// </summary>
    public Task<IEnumerable<Event>> GetBySeverityAsync(EventSeverity severity)
    {
        var events = _events
            .Where(e => e.Severity == severity)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
        
        return Task.FromResult<IEnumerable<Event>>(events);
    }

    /// <summary>
    /// Gets events with multiple filters.
    /// </summary>
    public Task<IEnumerable<Event>> GetFilteredAsync(
        string? robotId = null,
        string? eventType = null,
        EventSeverity? severity = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrEmpty(robotId))
        {
            query = query.Where(e => e.RobotId == robotId);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        }

        if (severity.HasValue)
        {
            query = query.Where(e => e.Severity == severity.Value);
        }

        if (startTime.HasValue)
        {
            query = query.Where(e => e.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(e => e.Timestamp <= endTime.Value);
        }

        var events = query.OrderByDescending(e => e.Timestamp).ToList();
        
        return Task.FromResult<IEnumerable<Event>>(events);
    }
}
