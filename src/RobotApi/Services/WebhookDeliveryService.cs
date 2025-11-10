using System.Text;
using System.Text.Json;
using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for delivering webhooks with exponential backoff retry logic.
/// </summary>
public sealed class WebhookDeliveryService : IWebhookDeliveryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDeliveryService> _logger;
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = { 1000, 2000, 4000 };

    public WebhookDeliveryService(HttpClient httpClient, ILogger<WebhookDeliveryService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _logger = logger;
    }

    public async Task<bool> DeliverWebhookAsync(Subscription subscription, Event evt)
    {
        var payload = new WebhookPayload
        {
            Event = EventResponse.FromEvent(evt),
            SubscriptionId = subscription.Id,
            DeliveredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug(
                    "Attempting webhook delivery to {WebhookUrl} for subscription {SubscriptionId}, attempt {Attempt}/{MaxRetries}",
                    subscription.WebhookUrl, subscription.Id, attempt + 1, MaxRetries);

                var response = await _httpClient.PostAsync(subscription.WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully delivered webhook to {WebhookUrl} for subscription {SubscriptionId}, event {EventId}, status code {StatusCode}",
                        subscription.WebhookUrl, subscription.Id, evt.Id, (int)response.StatusCode);
                    return true;
                }

                _logger.LogWarning(
                    "Webhook delivery failed to {WebhookUrl} for subscription {SubscriptionId}, event {EventId}, attempt {Attempt}/{MaxRetries}, status code {StatusCode}",
                    subscription.WebhookUrl, subscription.Id, evt.Id, attempt + 1, MaxRetries, (int)response.StatusCode);

                // If not the last attempt, wait before retrying
                if (attempt < MaxRetries - 1)
                {
                    await Task.Delay(RetryDelaysMs[attempt]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception during webhook delivery to {WebhookUrl} for subscription {SubscriptionId}, event {EventId}, attempt {Attempt}/{MaxRetries}",
                    subscription.WebhookUrl, subscription.Id, evt.Id, attempt + 1, MaxRetries);

                // If not the last attempt, wait before retrying
                if (attempt < MaxRetries - 1)
                {
                    await Task.Delay(RetryDelaysMs[attempt]);
                }
            }
        }

        _logger.LogError(
            "Failed to deliver webhook to {WebhookUrl} for subscription {SubscriptionId}, event {EventId} after {MaxRetries} attempts",
            subscription.WebhookUrl, subscription.Id, evt.Id, MaxRetries);

        return false;
    }
}
