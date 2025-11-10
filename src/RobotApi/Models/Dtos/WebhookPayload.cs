namespace RobotApi.Models.Dtos;

/// <summary>
/// Payload delivered to webhook endpoints.
/// </summary>
public class WebhookPayload
{
    /// <summary>
    /// The event that triggered this webhook.
    /// </summary>
    public EventResponse Event { get; set; } = null!;

    /// <summary>
    /// ID of the subscription that triggered this delivery.
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// When this payload was delivered (UTC).
    /// </summary>
    public DateTime DeliveredAt { get; set; }
}
