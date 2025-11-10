using System.Security.Claims;

namespace RobotApi.Middleware;

/// <summary>
/// Mock authentication middleware that simulates JWT bearer token authentication.
/// In production, this would be replaced with real JWT validation.
/// </summary>
public class MockAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MockAuthenticationMiddleware> _logger;

    // Mock operators for testing
    private static readonly Dictionary<string, (string Username, string Role)> MockTokens = new()
    {
        { "admin-token", ("admin", "Administrator") },
        { "operator-token", ("operator", "Operator") },
        { "viewer-token", ("viewer", "Viewer") }
    };

    public MockAuthenticationMiddleware(RequestDelegate next, ILogger<MockAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            if (MockTokens.TryGetValue(token, out var userInfo))
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Username),
                    new(ClaimTypes.Role, userInfo.Role),
                    new(ClaimTypes.NameIdentifier, userInfo.Username)
                };

                var identity = new ClaimsIdentity(claims, "MockAuth");
                context.User = new ClaimsPrincipal(identity);

                _logger.LogDebug("Authenticated user: {Username} with role: {Role}", userInfo.Username, userInfo.Role);
            }
            else
            {
                _logger.LogWarning("Invalid authentication token provided");
            }
        }

        await _next(context);
    }
}
