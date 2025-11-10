using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Service for delivering webhooks to subscribers.
/// </summary>
public interface IWebhookDeliveryService
{
    /// <summary>
    /// Delivers an event to a subscription's webhook URL with retry logic.
    /// </summary>
    Task<bool> DeliverWebhookAsync(Subscription subscription, Event evt);
}
