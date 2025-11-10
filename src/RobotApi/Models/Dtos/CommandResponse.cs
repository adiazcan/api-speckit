namespace RobotApi.Models.Dtos;

/// <summary>
/// Response DTO for command submission and status queries.
/// </summary>
public sealed class CommandResponse
{
    /// <summary>
    /// Gets or sets the unique command identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the target robot ID.
    /// </summary>
    public required string RobotId { get; set; }

    /// <summary>
    /// Gets or sets the command type.
    /// </summary>
    public required string CommandType { get; set; }

    /// <summary>
    /// Gets or sets the command-specific parameters.
    /// </summary>
    public required object Parameters { get; set; }

    /// <summary>
    /// Gets or sets the command priority level.
    /// </summary>
    public required CommandPriority Priority { get; set; }

    /// <summary>
    /// Gets or sets the current command status.
    /// </summary>
    public required CommandStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the command was created.
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when command execution started (null if not started).
    /// </summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when command execution finished (null if not finished).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the operator who issued the command.
    /// </summary>
    public required string OperatorId { get; set; }

    /// <summary>
    /// Gets or sets the execution result data (null if not completed).
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Gets or sets the error message if status is Failed (null otherwise).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a CommandResponse from a Command entity.
    /// </summary>
    public static CommandResponse FromCommand(Command command)
    {
        return new CommandResponse
        {
            Id = command.Id,
            RobotId = command.RobotId,
            CommandType = command.CommandType,
            Parameters = command.Parameters,
            Priority = command.Priority,
            Status = command.Status,
            CreatedAt = command.CreatedAt,
            ExecutedAt = command.ExecutedAt,
            CompletedAt = command.CompletedAt,
            OperatorId = command.OperatorId,
            Result = command.Result,
            ErrorMessage = command.ErrorMessage
        };
    }
}
