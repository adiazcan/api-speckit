using System.Text.Json;
using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for command management and execution coordination.
/// </summary>
public sealed class CommandService : ICommandService
{
    private readonly CommandStore _commandStore;
    private readonly RobotStore _robotStore;
    private readonly ILogger<CommandService> _logger;

    public CommandService(
        CommandStore commandStore,
        RobotStore robotStore,
        ILogger<CommandService> logger)
    {
        _commandStore = commandStore;
        _robotStore = robotStore;
        _logger = logger;
    }

    public async Task<Command> SendCommandAsync(string robotId, CommandRequest request, string operatorId)
    {
        // Validate robot exists
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            throw new KeyNotFoundException($"Robot with ID '{robotId}' not found");
        }

        // Check if robot is online
        if (robot.ConnectionStatus == ConnectionStatus.Offline)
        {
            throw new InvalidOperationException($"Robot '{robotId}' is offline and cannot accept commands");
        }

        // Validate command is supported by robot
        var commandType = request.CommandType.ToLowerInvariant();
        if (robot.Capabilities?.Commands == null || 
            !robot.Capabilities.Commands.Contains(commandType))
        {
            throw new InvalidOperationException(
                $"Robot '{robotId}' does not support command type '{request.CommandType}'");
        }

        // Validate speed against robot's max speed for move commands
        if (commandType == "move" && request.Parameters is JsonElement jsonElement)
        {
            try
            {
                var moveParams = JsonSerializer.Deserialize<MoveCommandParameters>(jsonElement.GetRawText());
                if (moveParams != null && 
                    robot.Capabilities.MaxSpeed.HasValue && 
                    moveParams.Speed > robot.Capabilities.MaxSpeed.Value)
                {
                    throw new InvalidOperationException(
                        $"Speed {moveParams.Speed} m/s exceeds robot's maximum speed of {robot.Capabilities.MaxSpeed.Value} m/s");
                }
            }
            catch (JsonException)
            {
                // If parsing fails, validator will catch it
            }
        }

        // Create command
        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            RobotId = robotId,
            CommandType = commandType,
            Parameters = request.Parameters,
            Priority = request.Priority,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = operatorId
        };

        await _commandStore.AddAsync(command);

        _logger.LogInformation(
            "Command {CommandId} ({CommandType}) created for robot {RobotId} by operator {OperatorId}",
            command.Id, command.CommandType, robotId, operatorId);

        return command;
    }

    public async Task<Command?> GetCommandAsync(string commandId)
    {
        return await _commandStore.GetByIdAsync(commandId);
    }

    public async Task<IEnumerable<Command>> ListCommandsAsync(string robotId, CommandStatus? status = null)
    {
        return await _commandStore.GetByRobotIdAndStatusAsync(robotId, status);
    }

    public async Task<bool> CancelCommandAsync(string commandId)
    {
        var command = await _commandStore.GetByIdAsync(commandId);
        if (command == null)
        {
            return false;
        }

        // Can only cancel pending commands
        if (command.Status != CommandStatus.Pending)
        {
            _logger.LogWarning(
                "Cannot cancel command {CommandId} with status {Status}",
                commandId, command.Status);
            return false;
        }

        // Transition to Failed with cancellation message
        command.Status = CommandStatus.Failed;
        command.CompletedAt = DateTime.UtcNow;
        command.ErrorMessage = "Command cancelled by operator";

        await _commandStore.UpdateAsync(commandId, command);

        _logger.LogInformation("Command {CommandId} cancelled", commandId);

        return true;
    }
}
