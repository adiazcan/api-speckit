using RobotApi.Data;
using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Service for managing events.
/// </summary>
public sealed class EventService : IEventService
{
    private readonly EventStore _eventStore;
    private readonly ILogger<EventService> _logger;

    public EventService(EventStore eventStore, ILogger<EventService> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Event> CreateEventAsync(
        string robotId, 
        string eventType, 
        EventSeverity severity, 
        string message, 
        Dictionary<string, object>? data = null)
    {
        var evt = new Event
        {
            Id = Guid.NewGuid().ToString(),
            RobotId = robotId,
            EventType = eventType,
            Severity = severity,
            Timestamp = DateTime.UtcNow,
            Message = message,
            Data = data ?? new Dictionary<string, object>()
        };

        await _eventStore.AddAsync(evt);

        _logger.LogInformation(
            "Created event {EventId} of type {EventType} for robot {RobotId} with severity {Severity}",
            evt.Id, evt.EventType, evt.RobotId, evt.Severity);

        return evt;
    }

    public Task<Event?> GetEventAsync(string id)
    {
        return _eventStore.GetByIdAsync(id);
    }

    public Task<IEnumerable<Event>> ListEventsAsync(
        string? robotId = null,
        string? eventType = null,
        EventSeverity? severity = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        return _eventStore.GetFilteredAsync(robotId, eventType, severity, startTime, endTime);
    }

    public Task<IEnumerable<Event>> GetEventsByRobotIdAsync(string robotId)
    {
        return _eventStore.GetByRobotIdAsync(robotId);
    }
}
