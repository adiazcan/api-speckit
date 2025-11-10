using System.Security.Claims;
using RobotApi.Models;

namespace RobotApi.Extensions;

/// <summary>
/// Extension methods for authentication and authorization.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Gets the current operator's username from claims.
    /// </summary>
    public static string? GetOperatorUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the current operator's ID from claims.
    /// </summary>
    public static string? GetOperatorId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the current operator's role from claims.
    /// </summary>
    public static string? GetOperatorRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Checks if the current user has the specified role or higher.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal user, OperatorRole requiredRole)
    {
        var roleString = user.GetOperatorRole();
        if (string.IsNullOrEmpty(roleString))
        {
            return false;
        }

        if (!Enum.TryParse<OperatorRole>(roleString, out var userRole))
        {
            return false;
        }

        return userRole >= requiredRole;
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated ?? false;
    }
}
