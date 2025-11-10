namespace RobotApi.Models;

/// <summary>
/// Represents a webhook subscription for robot events.
/// </summary>
public sealed class Subscription : IEntity
{
    /// <summary>
    /// Unique identifier for the subscription.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the operator who created this subscription.
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

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
    /// Whether this subscription is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional filter criteria for additional event filtering.
    /// </summary>
    public Dictionary<string, string>? FilterCriteria { get; set; }

    /// <summary>
    /// When the subscription was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
