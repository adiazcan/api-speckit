using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory data store for clients with seeded test data.
/// </summary>
public class ClientStore : IDataStore<Client>
{
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ILogger<ClientStore> _logger;

    public ClientStore(ILogger<ClientStore> logger)
    {
        _logger = logger;
        SeedData();
    }

    private void SeedData()
    {
        var clients = new[]
        {
            new Client
            {
                Id = "client-1",
                Name = "Acme Corporation",
                Email = "contact@acme.com",
                Phone = "+1-555-0100",
                Company = "Acme Corporation",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new Client
            {
                Id = "client-2",
                Name = "TechStart Inc",
                Email = "hello@techstart.io",
                Phone = "+1-555-0200",
                Company = "TechStart Inc",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Client
            {
                Id = "client-3",
                Name = "Global Logistics Ltd",
                Email = "info@globallogistics.com",
                Phone = "+1-555-0300",
                Company = "Global Logistics Ltd",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        foreach (var client in clients)
        {
            _clients.TryAdd(client.Id, client);
        }

        _logger.LogInformation("Seeded {Count} clients", clients.Length);
    }

    public Task<Client?> GetByIdAsync(string id)
    {
        _clients.TryGetValue(id, out var client);
        return Task.FromResult(client);
    }

    public Task<IEnumerable<Client>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Client>>(_clients.Values.ToList());
    }

    public Task<Client> AddAsync(Client entity)
    {
        if (!_clients.TryAdd(entity.Id, entity))
        {
            throw new InvalidOperationException($"Client with ID '{entity.Id}' already exists");
        }
        return Task.FromResult(entity);
    }

    public Task<Client?> UpdateAsync(string id, Client entity)
    {
        if (!_clients.ContainsKey(id))
        {
            return Task.FromResult<Client?>(null);
        }

        entity.UpdatedAt = DateTime.UtcNow;
        _clients[id] = entity;
        return Task.FromResult<Client?>(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _clients.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_clients.ContainsKey(id));
    }
}
