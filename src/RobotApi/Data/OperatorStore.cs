using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory data store for operators with seeded test data.
/// </summary>
public class OperatorStore : IDataStore<Operator>
{
    private readonly ConcurrentDictionary<string, Operator> _operators = new();
    private readonly ILogger<OperatorStore> _logger;

    public OperatorStore(ILogger<OperatorStore> logger)
    {
        _logger = logger;
        SeedData();
    }

    private void SeedData()
    {
        var operators = new[]
        {
            new Operator
            {
                Id = "admin",
                Username = "admin",
                Email = "admin@example.com",
                Role = OperatorRole.Administrator,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Operator
            {
                Id = "operator",
                Username = "operator",
                Email = "operator@example.com",
                Role = OperatorRole.Operator,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Operator
            {
                Id = "viewer",
                Username = "viewer",
                Email = "viewer@example.com",
                Role = OperatorRole.Viewer,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        foreach (var op in operators)
        {
            _operators.TryAdd(op.Id, op);
        }

        _logger.LogInformation("Seeded {Count} operators", operators.Length);
    }

    public Task<Operator?> GetByIdAsync(string id)
    {
        _operators.TryGetValue(id, out var op);
        return Task.FromResult(op);
    }

    public Task<IEnumerable<Operator>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Operator>>(_operators.Values.ToList());
    }

    public Task<Operator> AddAsync(Operator entity)
    {
        if (_operators.TryAdd(entity.Id, entity))
        {
            _logger.LogInformation("Added operator: {OperatorId}", entity.Id);
            return Task.FromResult(entity);
        }

        throw new InvalidOperationException($"Operator with ID {entity.Id} already exists");
    }

    public Task<Operator?> UpdateAsync(string id, Operator entity)
    {
        if (_operators.TryGetValue(id, out var existing))
        {
            entity.Id = id; // Ensure ID doesn't change
            _operators[id] = entity;
            _logger.LogInformation("Updated operator: {OperatorId}", id);
            return Task.FromResult<Operator?>(entity);
        }

        return Task.FromResult<Operator?>(null);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _operators.TryRemove(id, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted operator: {OperatorId}", id);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_operators.ContainsKey(id));
    }
}
