using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service interface for client management operations.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Creates a new client.
    /// </summary>
    Task<Client> CreateClientAsync(ClientRequest request);

    /// <summary>
    /// Gets a client by ID.
    /// </summary>
    Task<Client?> GetClientAsync(string clientId);

    /// <summary>
    /// Lists all clients.
    /// </summary>
    Task<IEnumerable<Client>> ListClientsAsync();

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    Task<Client?> UpdateClientAsync(string clientId, ClientRequest request);

    /// <summary>
    /// Deletes a client.
    /// </summary>
    Task<bool> DeleteClientAsync(string clientId);
}
