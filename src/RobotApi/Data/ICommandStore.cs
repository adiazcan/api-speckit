using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// Interface for Command data store with specialized query methods.
/// </summary>
public interface ICommandStore : IDataStore<Command>
{
    /// <summary>
    /// Gets all commands for a specific robot, ordered by creation date descending.
    /// </summary>
    Task<IEnumerable<Command>> GetByRobotIdAsync(string robotId);

    /// <summary>
    /// Gets commands for a robot filtered by status.
    /// </summary>
    Task<IEnumerable<Command>> GetByRobotIdAndStatusAsync(string robotId, CommandStatus? status = null);

    /// <summary>
    /// Gets pending commands ordered by priority (Emergency > High > Normal) then by creation date.
    /// </summary>
    Task<IEnumerable<Command>> GetPendingCommandsAsync();
}
