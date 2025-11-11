namespace RobotApi.Models.Dtos;

/// <summary>
/// Response model for client data.
/// </summary>
public class ClientResponse
{
    /// <summary>
    /// Unique client identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Client name.
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
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when client was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Converts a Client entity to a ClientResponse.
    /// </summary>
    public static ClientResponse FromClient(Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Company = client.Company,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }
}
