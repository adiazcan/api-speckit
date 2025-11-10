using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Service for managing events.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new event.
    /// </summary>
    Task<Event> CreateEventAsync(string robotId, string eventType, EventSeverity severity, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Gets an event by ID.
    /// </summary>
    Task<Event?> GetEventAsync(string id);

    /// <summary>
    /// Lists events with optional filtering.
    /// </summary>
    Task<IEnumerable<Event>> ListEventsAsync(
        string? robotId = null,
        string? eventType = null,
        EventSeverity? severity = null,
        DateTime? startTime = null,
        DateTime? endTime = null);

    /// <summary>
    /// Gets events by robot ID.
    /// </summary>
    Task<IEnumerable<Event>> GetEventsByRobotIdAsync(string robotId);
}
