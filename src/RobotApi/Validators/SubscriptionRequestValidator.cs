using FluentValidation;
using RobotApi.Models.Dtos;

namespace RobotApi.Validators;

/// <summary>
/// Validator for SubscriptionRequest.
/// </summary>
public class SubscriptionRequestValidator : AbstractValidator<SubscriptionRequest>
{
    private static readonly string[] ValidEventTypes = 
    {
        "command_completed",
        "command_failed",
        "battery_low",
        "sensor_threshold",
        "error"
    };

    public SubscriptionRequestValidator()
    {
        RuleFor(x => x.WebhookUrl)
            .NotEmpty()
            .WithMessage("WebhookUrl is required")
            .Must(BeValidHttpsUrl)
            .WithMessage("WebhookUrl must be a valid HTTPS URL");

        RuleFor(x => x.EventTypes)
            .NotEmpty()
            .WithMessage("At least one event type must be specified")
            .Must(HaveValidEventTypes)
            .WithMessage($"Event types must be one of: {string.Join(", ", ValidEventTypes)}");
    }

    private bool BeValidHttpsUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps;
    }

    private bool HaveValidEventTypes(string[] eventTypes)
    {
        if (eventTypes == null || eventTypes.Length == 0)
        {
            return false;
        }

        return eventTypes.All(et => ValidEventTypes.Contains(et, StringComparer.OrdinalIgnoreCase));
    }
}
