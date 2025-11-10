using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for managing subscriptions.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Creates a new subscription.
    /// </summary>
    Task<Subscription> CreateSubscriptionAsync(string operatorId, SubscriptionRequest request);

    /// <summary>
    /// Gets a subscription by ID.
    /// </summary>
    Task<Subscription?> GetSubscriptionAsync(string id);

    /// <summary>
    /// Lists subscriptions for an operator.
    /// </summary>
    Task<IEnumerable<Subscription>> ListSubscriptionsAsync(string operatorId);

    /// <summary>
    /// Updates a subscription.
    /// </summary>
    Task<Subscription?> UpdateSubscriptionAsync(string id, string operatorId, SubscriptionRequest request);

    /// <summary>
    /// Deletes a subscription.
    /// </summary>
    Task<bool> DeleteSubscriptionAsync(string id, string operatorId);

    /// <summary>
    /// Gets active subscriptions that match the given event.
    /// </summary>
    Task<IEnumerable<Subscription>> GetMatchingSubscriptionsAsync(Event evt);
}
