using FluentValidation;
using RobotApi.Models.Dtos;

namespace RobotApi.Validators;

/// <summary>
/// Validator for TelemetryHistoryRequest with 90-day limit enforcement.
/// </summary>
public class TelemetryHistoryRequestValidator : AbstractValidator<TelemetryHistoryRequest>
{
    private const int MaxTimeRangeDays = 90;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 1000;

    public TelemetryHistoryRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("StartTime is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("StartTime cannot be in the future")
            .LessThan(x => x.EndTime)
            .WithMessage("StartTime must be before EndTime");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("EndTime is required");

        RuleFor(x => x)
            .Must(request => (request.EndTime - request.StartTime).TotalDays <= MaxTimeRangeDays)
            .WithMessage($"Time range cannot exceed {MaxTimeRangeDays} days")
            .WithName("TimeRange");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(MinPageSize, MaxPageSize)
            .WithMessage($"PageSize must be between {MinPageSize} and {MaxPageSize}");
    }
}
