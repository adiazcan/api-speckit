using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory store for subscriptions with active subscription queries.
/// </summary>
public sealed class SubscriptionStore : IDataStore<Subscription>
{
    private readonly ConcurrentDictionary<string, Subscription> _subscriptions = new();
    private readonly ILogger<SubscriptionStore> _logger;

    public SubscriptionStore(ILogger<SubscriptionStore> logger)
    {
        _logger = logger;
    }

    public Task<Subscription?> GetByIdAsync(string id)
    {
        _subscriptions.TryGetValue(id, out var subscription);
        return Task.FromResult(subscription);
    }

    public Task<IEnumerable<Subscription>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Subscription>>(_subscriptions.Values.ToList());
    }

    public Task<Subscription> AddAsync(Subscription entity)
    {
        if (_subscriptions.TryAdd(entity.Id, entity))
        {
            _logger.LogDebug("Added subscription {SubscriptionId} for operator {OperatorId}", 
                entity.Id, entity.OperatorId);
        }
        return Task.FromResult(entity);
    }

    public Task<Subscription?> UpdateAsync(string id, Subscription entity)
    {
        _subscriptions[id] = entity;
        _logger.LogDebug("Updated subscription {SubscriptionId}", id);
        return Task.FromResult<Subscription?>(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var deleted = _subscriptions.TryRemove(id, out var subscription);
        if (deleted)
        {
            _logger.LogDebug("Deleted subscription {SubscriptionId}", id);
        }
        return Task.FromResult(deleted);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_subscriptions.ContainsKey(id));
    }

    /// <summary>
    /// Gets all active subscriptions.
    /// </summary>
    public Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync()
    {
        var activeSubscriptions = _subscriptions.Values
            .Where(s => s.IsActive)
            .ToList();
        
        return Task.FromResult<IEnumerable<Subscription>>(activeSubscriptions);
    }

    /// <summary>
    /// Gets subscriptions by operator ID.
    /// </summary>
    public Task<IEnumerable<Subscription>> GetByOperatorIdAsync(string operatorId)
    {
        var subscriptions = _subscriptions.Values
            .Where(s => s.OperatorId == operatorId)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
        
        return Task.FromResult<IEnumerable<Subscription>>(subscriptions);
    }

    /// <summary>
    /// Gets active subscriptions that match the given event.
    /// </summary>
    public Task<IEnumerable<Subscription>> GetMatchingSubscriptionsAsync(Event evt)
    {
        var matchingSubscriptions = _subscriptions.Values
            .Where(s => s.IsActive)
            .Where(s => s.EventTypes.Contains(evt.EventType, StringComparer.OrdinalIgnoreCase))
            .Where(s => string.IsNullOrEmpty(s.RobotId) || s.RobotId == evt.RobotId)
            .ToList();
        
        return Task.FromResult<IEnumerable<Subscription>>(matchingSubscriptions);
    }
}
