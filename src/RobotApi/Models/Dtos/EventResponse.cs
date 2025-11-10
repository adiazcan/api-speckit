namespace RobotApi.Models.Dtos;

/// <summary>
/// Response model for event information.
/// </summary>
public class EventResponse
{
    public string Id { get; set; } = string.Empty;
    public string RobotId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();

    public static EventResponse FromEvent(Event evt)
    {
        return new EventResponse
        {
            Id = evt.Id,
            RobotId = evt.RobotId,
            EventType = evt.EventType,
            Severity = evt.Severity.ToString(),
            Timestamp = evt.Timestamp,
            Message = evt.Message,
            Data = evt.Data
        };
    }
}
