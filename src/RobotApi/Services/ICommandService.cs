using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service interface for command management and execution.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Sends a new command to a robot.
    /// </summary>
    /// <param name="robotId">The target robot ID.</param>
    /// <param name="request">The command request details.</param>
    /// <param name="operatorId">The ID of the operator issuing the command.</param>
    /// <returns>The created command with status Pending.</returns>
    Task<Command> SendCommandAsync(string robotId, CommandRequest request, string operatorId);

    /// <summary>
    /// Gets a specific command by ID.
    /// </summary>
    /// <param name="commandId">The command ID to retrieve.</param>
    /// <returns>The command if found, otherwise null.</returns>
    Task<Command?> GetCommandAsync(string commandId);

    /// <summary>
    /// Gets all commands for a specific robot.
    /// </summary>
    /// <param name="robotId">The robot ID to filter by.</param>
    /// <param name="status">Optional status filter.</param>
    /// <returns>List of commands ordered by creation date descending.</returns>
    Task<IEnumerable<Command>> ListCommandsAsync(string robotId, CommandStatus? status = null);

    /// <summary>
    /// Cancels a pending command.
    /// </summary>
    /// <param name="commandId">The command ID to cancel.</param>
    /// <returns>True if cancelled successfully, false if not found or already executing/completed.</returns>
    Task<bool> CancelCommandAsync(string commandId);
}
