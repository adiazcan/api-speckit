using RobotApi.Data;

namespace RobotApi.Services;

/// <summary>
/// Background service that monitors events and delivers webhooks to matching subscriptions.
/// </summary>
public sealed class WebhookProcessorService : BackgroundService
{
    private readonly EventStore _eventStore;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IWebhookDeliveryService _webhookDeliveryService;
    private readonly ILogger<WebhookProcessorService> _logger;
    private readonly HashSet<string> _processedEventIds = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private const int PollingIntervalMs = 2000;

    public WebhookProcessorService(
        EventStore eventStore,
        ISubscriptionService subscriptionService,
        IWebhookDeliveryService webhookDeliveryService,
        ILogger<WebhookProcessorService> logger)
    {
        _eventStore = eventStore;
        _subscriptionService = subscriptionService;
        _webhookDeliveryService = webhookDeliveryService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebhookProcessorService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending events");
            }

            await Task.Delay(PollingIntervalMs, stoppingToken);
        }

        _logger.LogInformation("WebhookProcessorService stopped");
    }

    private async Task ProcessPendingEventsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Get recent events (last 5 minutes to handle any missed events)
            var startTime = DateTime.UtcNow.AddMinutes(-5);
            var events = await _eventStore.GetFilteredAsync(startTime: startTime);

            foreach (var evt in events)
            {
                // Skip already processed events
                if (_processedEventIds.Contains(evt.Id))
                {
                    continue;
                }

                // Get matching subscriptions
                var subscriptions = await _subscriptionService.GetMatchingSubscriptionsAsync(evt);

                // Deliver to each matching subscription
                var deliveryTasks = subscriptions.Select(subscription =>
                    DeliverToSubscriptionAsync(subscription, evt));

                await Task.WhenAll(deliveryTasks);

                // Mark event as processed
                _processedEventIds.Add(evt.Id);

                // Clean up old processed event IDs (keep last 1000)
                if (_processedEventIds.Count > 1000)
                {
                    var toRemove = _processedEventIds.Take(_processedEventIds.Count - 1000).ToList();
                    foreach (var id in toRemove)
                    {
                        _processedEventIds.Remove(id);
                    }
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DeliverToSubscriptionAsync(Models.Subscription subscription, Models.Event evt)
    {
        try
        {
            var success = await _webhookDeliveryService.DeliverWebhookAsync(subscription, evt);

            if (!success)
            {
                _logger.LogWarning(
                    "Failed to deliver event {EventId} to subscription {SubscriptionId} after all retries",
                    evt.Id, subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception delivering event {EventId} to subscription {SubscriptionId}",
                evt.Id, subscription.Id);
        }
    }
}
