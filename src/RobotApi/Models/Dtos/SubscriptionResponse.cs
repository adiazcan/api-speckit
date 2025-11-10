namespace RobotApi.Models.Dtos;

/// <summary>
/// Response model for subscription information.
/// </summary>
public class SubscriptionResponse
{
    public string Id { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public string? RobotId { get; set; }
    public string[] EventTypes { get; set; } = Array.Empty<string>();
    public string WebhookUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Dictionary<string, string>? FilterCriteria { get; set; }
    public DateTime CreatedAt { get; set; }

    public static SubscriptionResponse FromSubscription(Subscription subscription)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            OperatorId = subscription.OperatorId,
            RobotId = subscription.RobotId,
            EventTypes = subscription.EventTypes,
            WebhookUrl = subscription.WebhookUrl,
            IsActive = subscription.IsActive,
            FilterCriteria = subscription.FilterCriteria,
            CreatedAt = subscription.CreatedAt
        };
    }
}
