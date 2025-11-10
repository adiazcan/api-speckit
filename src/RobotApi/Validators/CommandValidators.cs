using FluentValidation;
using RobotApi.Models.Dtos;

namespace RobotApi.Validators;

/// <summary>
/// Validator for CommandRequest DTO.
/// </summary>
public sealed class CommandRequestValidator : AbstractValidator<CommandRequest>
{
    private static readonly string[] ValidCommandTypes = { "move", "rotate", "stop", "sensor_activate" };

    public CommandRequestValidator()
    {
        RuleFor(x => x.CommandType)
            .NotEmpty()
            .WithMessage("CommandType is required")
            .Must(type => ValidCommandTypes.Contains(type.ToLowerInvariant()))
            .WithMessage($"CommandType must be one of: {string.Join(", ", ValidCommandTypes)}");

        RuleFor(x => x.Parameters)
            .NotNull()
            .WithMessage("Parameters are required");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be a valid CommandPriority value");
    }
}

/// <summary>
/// Validator for MoveCommandParameters.
/// </summary>
public sealed class MoveCommandParametersValidator : AbstractValidator<MoveCommandParameters>
{
    private static readonly string[] ValidDirections = { "forward", "backward", "left", "right" };

    public MoveCommandParametersValidator()
    {
        RuleFor(x => x.Direction)
            .NotEmpty()
            .WithMessage("Direction is required")
            .Must(dir => ValidDirections.Contains(dir.ToLowerInvariant()))
            .WithMessage($"Direction must be one of: {string.Join(", ", ValidDirections)}");

        RuleFor(x => x.Distance)
            .GreaterThan(0)
            .WithMessage("Distance must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Distance cannot exceed 100 meters");

        RuleFor(x => x.Speed)
            .GreaterThan(0)
            .WithMessage("Speed must be greater than 0");
    }
}

/// <summary>
/// Validator for RotateCommandParameters.
/// </summary>
public sealed class RotateCommandParametersValidator : AbstractValidator<RotateCommandParameters>
{
    private static readonly string[] ValidDirections = { "left", "right" };

    public RotateCommandParametersValidator()
    {
        RuleFor(x => x.Direction)
            .NotEmpty()
            .WithMessage("Direction is required")
            .Must(dir => ValidDirections.Contains(dir.ToLowerInvariant()))
            .WithMessage($"Direction must be one of: {string.Join(", ", ValidDirections)}");

        RuleFor(x => x.Degrees)
            .GreaterThan(0)
            .WithMessage("Degrees must be greater than 0")
            .LessThanOrEqualTo(360)
            .WithMessage("Degrees cannot exceed 360");
    }
}

/// <summary>
/// Validator for StopCommandParameters (no validation needed, but included for consistency).
/// </summary>
public sealed class StopCommandParametersValidator : AbstractValidator<StopCommandParameters>
{
    public StopCommandParametersValidator()
    {
        // Stop command has no parameters to validate
    }
}

/// <summary>
/// Validator for SensorActivateCommandParameters.
/// </summary>
public sealed class SensorActivateCommandParametersValidator : AbstractValidator<SensorActivateCommandParameters>
{
    public SensorActivateCommandParametersValidator()
    {
        RuleFor(x => x.SensorName)
            .NotEmpty()
            .WithMessage("SensorName is required")
            .MaximumLength(50)
            .WithMessage("SensorName cannot exceed 50 characters");

        RuleFor(x => x.Enabled)
            .NotNull()
            .WithMessage("Enabled flag is required");
    }
}
