namespace RobotApi.Models.Dtos;

/// <summary>
/// Request model for creating or updating a client.
/// </summary>
public class ClientRequest
{
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
}
