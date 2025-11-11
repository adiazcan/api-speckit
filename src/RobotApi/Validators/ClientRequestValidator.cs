using FluentValidation;
using RobotApi.Models.Dtos;

namespace RobotApi.Validators;

/// <summary>
/// Validator for client requests.
/// </summary>
public sealed class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(1, 100).WithMessage("Name must be between 1 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Company)
            .MaximumLength(100).WithMessage("Company must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Company));
    }
}
