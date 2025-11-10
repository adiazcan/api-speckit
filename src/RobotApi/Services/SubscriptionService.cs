using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for managing subscriptions.
/// </summary>
public sealed class SubscriptionService : ISubscriptionService
{
    private readonly SubscriptionStore _subscriptionStore;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(SubscriptionStore subscriptionStore, ILogger<SubscriptionService> logger)
    {
        _subscriptionStore = subscriptionStore;
        _logger = logger;
    }

    public async Task<Subscription> CreateSubscriptionAsync(string operatorId, SubscriptionRequest request)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid().ToString(),
            OperatorId = operatorId,
            RobotId = request.RobotId,
            EventTypes = request.EventTypes,
            WebhookUrl = request.WebhookUrl,
            IsActive = true,
            FilterCriteria = request.FilterCriteria ?? new Dictionary<string, string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _subscriptionStore.AddAsync(subscription);

        _logger.LogInformation(
            "Created subscription {SubscriptionId} for operator {OperatorId} with webhook {WebhookUrl}",
            subscription.Id, subscription.OperatorId, subscription.WebhookUrl);

        return subscription;
    }

    public Task<Subscription?> GetSubscriptionAsync(string id)
    {
        return _subscriptionStore.GetByIdAsync(id);
    }

    public Task<IEnumerable<Subscription>> ListSubscriptionsAsync(string operatorId)
    {
        return _subscriptionStore.GetByOperatorIdAsync(operatorId);
    }

    public async Task<Subscription?> UpdateSubscriptionAsync(string id, string operatorId, SubscriptionRequest request)
    {
        var subscription = await _subscriptionStore.GetByIdAsync(id);
        
        if (subscription == null)
        {
            return null;
        }

        // Verify ownership
        if (subscription.OperatorId != operatorId)
        {
            _logger.LogWarning(
                "Operator {OperatorId} attempted to update subscription {SubscriptionId} owned by {OwnerId}",
                operatorId, id, subscription.OperatorId);
            return null;
        }

        // Update subscription
        subscription.RobotId = request.RobotId;
        subscription.EventTypes = request.EventTypes;
        subscription.WebhookUrl = request.WebhookUrl;
        subscription.FilterCriteria = request.FilterCriteria ?? new Dictionary<string, string>();

        await _subscriptionStore.UpdateAsync(id, subscription);

        _logger.LogInformation("Updated subscription {SubscriptionId}", subscription.Id);

        return subscription;
    }

    public async Task<bool> DeleteSubscriptionAsync(string id, string operatorId)
    {
        var subscription = await _subscriptionStore.GetByIdAsync(id);
        
        if (subscription == null)
        {
            return false;
        }

        // Verify ownership
        if (subscription.OperatorId != operatorId)
        {
            _logger.LogWarning(
                "Operator {OperatorId} attempted to delete subscription {SubscriptionId} owned by {OwnerId}",
                operatorId, id, subscription.OperatorId);
            return false;
        }

        await _subscriptionStore.DeleteAsync(id);

        _logger.LogInformation("Deleted subscription {SubscriptionId}", id);

        return true;
    }

    public Task<IEnumerable<Subscription>> GetMatchingSubscriptionsAsync(Event evt)
    {
        return _subscriptionStore.GetMatchingSubscriptionsAsync(evt);
    }
}
