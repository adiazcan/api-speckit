using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace RobotApi.Extensions;

/// <summary>
/// Extension methods for integrating FluentValidation with ASP.NET Core.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation validators from the specified assembly.
    /// </summary>
    public static IServiceCollection AddFluentValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    /// <summary>
    /// Converts FluentValidation errors to a ProblemDetails response.
    /// </summary>
    public static ProblemDetails ToProblemDetails(this ValidationResult validationResult, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred",
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        problemDetails.Extensions["errors"] = errors;

        return problemDetails;
    }

    /// <summary>
    /// Validates an object using its registered FluentValidation validator.
    /// </summary>
    public static async Task<ValidationResult> ValidateAsync<T>(
        this T instance,
        IValidator<T> validator)
    {
        return await validator.ValidateAsync(instance);
    }
}
