namespace RobotApi.Models.Dtos;

/// <summary>
/// Request model for creating a new subscription.
/// </summary>
public class SubscriptionRequest
{
    /// <summary>
    /// Optional robot ID to filter events (null means all robots).
    /// </summary>
    public string? RobotId { get; set; }

    /// <summary>
    /// Array of event types to subscribe to (e.g., "command_completed", "battery_low").
    /// </summary>
    public string[] EventTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Webhook URL to receive event notifications (must be HTTPS).
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional filter criteria for additional event filtering.
    /// </summary>
    public Dictionary<string, string>? FilterCriteria { get; set; }
}
