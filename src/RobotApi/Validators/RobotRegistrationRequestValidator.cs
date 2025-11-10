using FluentValidation;
using RobotApi.Models.Dtos;

namespace RobotApi.Validators;

/// <summary>
/// Validator for robot registration requests.
/// </summary>
public sealed class RobotRegistrationRequestValidator : AbstractValidator<RobotRegistrationRequest>
{
    public RobotRegistrationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters")
            .Matches(@"^[a-zA-Z0-9\-_\s]+$").WithMessage("Name can only contain alphanumeric characters, hyphens, underscores, and spaces");

        RuleFor(x => x.ModelType)
            .NotEmpty().WithMessage("ModelType is required")
            .Length(3, 50).WithMessage("ModelType must be between 3 and 50 characters");

        RuleFor(x => x.FirmwareVersion)
            .NotEmpty().WithMessage("FirmwareVersion is required")
            .Must(BeValidSemanticVersion).WithMessage("FirmwareVersion must be a valid semantic version (e.g., 1.0.0, 2.1.3)");

        RuleFor(x => x.Capabilities)
            .NotNull().WithMessage("Capabilities are required")
            .ChildRules(capabilities =>
            {
                capabilities.RuleFor(c => c.Commands)
                    .NotNull().WithMessage("Commands list is required")
                    .Must(commands => commands != null && commands.Length > 0).WithMessage("At least one command must be specified");

                capabilities.RuleFor(c => c.Sensors)
                    .NotNull().WithMessage("Sensors list is required");

                capabilities.RuleFor(c => c.MaxSpeed)
                    .GreaterThan(0).When(c => c.MaxSpeed.HasValue)
                    .WithMessage("MaxSpeed must be greater than 0 if specified");

                capabilities.RuleFor(c => c.MaxDistance)
                    .GreaterThan(0).When(c => c.MaxDistance.HasValue)
                    .WithMessage("MaxDistance must be greater than 0 if specified");
            });
    }

    private bool BeValidSemanticVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        // Semantic versioning pattern: MAJOR.MINOR.PATCH (e.g., 1.0.0, 2.1.3, 10.20.30)
        var semverPattern = @"^\d+\.\d+\.\d+$";
        return System.Text.RegularExpressions.Regex.IsMatch(version, semverPattern);
    }
}
