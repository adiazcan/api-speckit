namespace RobotApi.Models;

/// <summary>
/// Represents a client in the system.
/// </summary>
public class Client : IEntity
{
    /// <summary>
    /// Unique client identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Client name (1-100 characters).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Client email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Client phone number (optional).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Client company name (optional).
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// UTC timestamp when client was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when client was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
