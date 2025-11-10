namespace RobotApi.Tests.Integration.Helpers;

/// <summary>
/// Helper class for generating test authentication tokens.
/// </summary>
public static class AuthHelper
{
    /// <summary>
    /// Gets a test token for the Administrator role.
    /// </summary>
    public static string GetAdminToken() => "admin-token";

    /// <summary>
    /// Gets a test token for the Operator role.
    /// </summary>
    public static string GetOperatorToken() => "operator-token";

    /// <summary>
    /// Gets a test token for the Viewer role.
    /// </summary>
    public static string GetViewerToken() => "viewer-token";

    /// <summary>
    /// Creates an authorization header value for the specified role.
    /// </summary>
    public static string GetAuthorizationHeader(string role)
    {
        var token = role.ToLowerInvariant() switch
        {
            "administrator" or "admin" => GetAdminToken(),
            "operator" => GetOperatorToken(),
            "viewer" => GetViewerToken(),
            _ => throw new ArgumentException($"Unknown role: {role}", nameof(role))
        };

        return $"Bearer {token}";
    }

    /// <summary>
    /// Adds authentication header to an HTTP client.
    /// </summary>
    public static void AddAuthentication(HttpClient client, string role)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", GetAuthorizationHeader(role));
    }
}
