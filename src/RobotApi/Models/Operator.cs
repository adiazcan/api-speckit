namespace RobotApi.Models;

/// <summary>
/// Represents a user with authentication credentials and permissions.
/// </summary>
public class Operator : IEntity
{
    /// <summary>
    /// Unique operator identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Login username (3-50 characters, alphanumeric plus underscore/hyphen).
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Contact email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Permission level for the operator.
    /// </summary>
    public required OperatorRole Role { get; set; }

    /// <summary>
    /// UTC timestamp of account creation.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of last login (nullable).
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Permission levels for operators.
/// </summary>
public enum OperatorRole
{
    /// <summary>
    /// Read telemetry and view robots (no commands).
    /// </summary>
    Viewer = 0,

    /// <summary>
    /// Viewer + send commands, manage subscriptions.
    /// </summary>
    Operator = 1,

    /// <summary>
    /// Operator + register/deregister robots, manage operators.
    /// </summary>
    Administrator = 2
}
