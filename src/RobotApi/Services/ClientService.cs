using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for client management operations.
/// </summary>
public sealed class ClientService : IClientService
{
    private readonly IDataStore<Client> _clientStore;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IDataStore<Client> clientStore, ILogger<ClientService> logger)
    {
        _clientStore = clientStore;
        _logger = logger;
    }

    public async Task<Client> CreateClientAsync(ClientRequest request)
    {
        // Check if client with same email already exists
        var existingClients = await _clientStore.GetAllAsync();
        if (existingClients.Any(c => c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A client with email '{request.Email}' already exists");
        }

        var client = new Client
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            CreatedAt = DateTime.UtcNow
        };

        await _clientStore.AddAsync(client);

        _logger.LogInformation(
            "Created new client {ClientId} with name '{ClientName}' and email '{Email}'",
            client.Id, client.Name, client.Email);

        return client;
    }

    public Task<Client?> GetClientAsync(string clientId)
    {
        return _clientStore.GetByIdAsync(clientId);
    }

    public Task<IEnumerable<Client>> ListClientsAsync()
    {
        return _clientStore.GetAllAsync();
    }

    public async Task<Client?> UpdateClientAsync(string clientId, ClientRequest request)
    {
        var client = await _clientStore.GetByIdAsync(clientId);
        if (client == null)
        {
            return null;
        }

        // Check if another client with the same email exists
        var existingClients = await _clientStore.GetAllAsync();
        if (existingClients.Any(c => c.Id != clientId && c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A client with email '{request.Email}' already exists");
        }

        client.Name = request.Name;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Company = request.Company;

        await _clientStore.UpdateAsync(clientId, client);

        _logger.LogInformation(
            "Updated client {ClientId} with name '{ClientName}'",
            clientId, client.Name);

        return client;
    }

    public async Task<bool> DeleteClientAsync(string clientId)
    {
        var client = await _clientStore.GetByIdAsync(clientId);
        if (client == null)
        {
            return false;
        }

        var deleted = await _clientStore.DeleteAsync(clientId);

        if (deleted)
        {
            _logger.LogInformation(
                "Deleted client {ClientId} with name '{ClientName}'",
                clientId, client.Name);
        }

        return deleted;
    }
}
