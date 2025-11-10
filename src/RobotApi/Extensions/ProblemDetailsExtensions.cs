using Microsoft.AspNetCore.Mvc;

namespace RobotApi.Extensions;

/// <summary>
/// Extension methods for creating RFC 7807 ProblemDetails responses.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Creates a ProblemDetails response for bad request (400).
    /// </summary>
    public static ProblemDetails BadRequest(string title, string detail, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for unauthorized (401).
    /// </summary>
    public static ProblemDetails Unauthorized(string title = "Unauthorized", string? detail = null, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = title,
            Detail = detail ?? "Authentication is required to access this resource",
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for forbidden (403).
    /// </summary>
    public static ProblemDetails Forbidden(string title = "Forbidden", string? detail = null, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = title,
            Detail = detail ?? "You do not have permission to access this resource",
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for not found (404).
    /// </summary>
    public static ProblemDetails NotFound(string title, string detail, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for conflict (409).
    /// </summary>
    public static ProblemDetails Conflict(string title, string detail, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for service unavailable (503).
    /// </summary>
    public static ProblemDetails ServiceUnavailable(string title, string detail, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails response for internal server error (500).
    /// </summary>
    public static ProblemDetails InternalServerError(string title = "Internal Server Error", string? detail = null, string? instance = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = title,
            Detail = detail ?? "An unexpected error occurred while processing your request",
            Instance = instance,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}
