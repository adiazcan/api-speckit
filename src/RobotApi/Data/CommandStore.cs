using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory data store for Command entities using thread-safe ConcurrentDictionary.
/// </summary>
public sealed class CommandStore : IDataStore<Command>
{
    private readonly ConcurrentDictionary<string, Command> _commands = new();
    private readonly ILogger<CommandStore> _logger;

    public CommandStore(ILogger<CommandStore> logger)
    {
        _logger = logger;
    }

    public Task<Command?> GetByIdAsync(string id)
    {
        _commands.TryGetValue(id, out var command);
        return Task.FromResult(command);
    }

    public Task<IEnumerable<Command>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Command>>(_commands.Values.ToList());
    }

    public Task<Command> AddAsync(Command entity)
    {
        if (_commands.TryAdd(entity.Id, entity))
        {
            _logger.LogDebug("Added command {CommandId} for robot {RobotId}", entity.Id, entity.RobotId);
            return Task.FromResult(entity);
        }

        throw new InvalidOperationException($"Command with ID '{entity.Id}' already exists");
    }

    public Task<Command?> UpdateAsync(string id, Command entity)
    {
        if (_commands.ContainsKey(id))
        {
            _commands[id] = entity;
            _logger.LogDebug("Updated command {CommandId} to status {Status}", id, entity.Status);
            return Task.FromResult<Command?>(entity);
        }

        throw new KeyNotFoundException($"Command with ID '{id}' not found");
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _commands.TryRemove(id, out _);
        if (removed)
        {
            _logger.LogDebug("Deleted command {CommandId}", id);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_commands.ContainsKey(id));
    }

    /// <summary>
    /// Gets all commands for a specific robot, ordered by creation date descending.
    /// </summary>
    public Task<IEnumerable<Command>> GetByRobotIdAsync(string robotId)
    {
        var commands = _commands.Values
            .Where(c => c.RobotId == robotId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return Task.FromResult<IEnumerable<Command>>(commands);
    }

    /// <summary>
    /// Gets commands for a robot filtered by status.
    /// </summary>
    public Task<IEnumerable<Command>> GetByRobotIdAndStatusAsync(string robotId, CommandStatus? status = null)
    {
        var query = _commands.Values
            .Where(c => c.RobotId == robotId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var commands = query
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return Task.FromResult<IEnumerable<Command>>(commands);
    }

    /// <summary>
    /// Gets pending commands ordered by priority (Emergency > High > Normal) then by creation date.
    /// </summary>
    public Task<IEnumerable<Command>> GetPendingCommandsAsync()
    {
        var commands = _commands.Values
            .Where(c => c.Status == CommandStatus.Pending)
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.CreatedAt)
            .ToList();

        return Task.FromResult<IEnumerable<Command>>(commands);
    }
}
