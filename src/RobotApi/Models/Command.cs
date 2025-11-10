namespace RobotApi.Models;

/// <summary>
/// Represents a control instruction sent to a robot.
/// </summary>
public sealed class Command : IEntity
{
    /// <summary>
    /// Gets or sets the unique command identifier (GUID).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the target robot ID (foreign key).
    /// </summary>
    public required string RobotId { get; set; }

    /// <summary>
    /// Gets or sets the command type (e.g., "move", "rotate", "stop", "sensor_activate").
    /// </summary>
    public required string CommandType { get; set; }

    /// <summary>
    /// Gets or sets the command-specific parameters as a JSON object.
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
    /// Gets or sets the UTC timestamp when command execution started.
    /// </summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when command execution finished.
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
    /// Gets or sets the error message if status is Failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validates state transition from current status to new status.
    /// </summary>
    /// <param name="newStatus">The target status.</param>
    /// <returns>True if transition is valid, false otherwise.</returns>
    public bool CanTransitionTo(CommandStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            // From Pending
            (CommandStatus.Pending, CommandStatus.Executing) => true,
            (CommandStatus.Pending, CommandStatus.Failed) => true,

            // From Executing
            (CommandStatus.Executing, CommandStatus.Completed) => true,
            (CommandStatus.Executing, CommandStatus.Failed) => true,

            // Terminal states cannot transition
            (CommandStatus.Completed, _) => false,
            (CommandStatus.Failed, _) => false,

            // All other transitions are invalid
            _ => false
        };
    }
}

/// <summary>
/// Command priority levels.
/// </summary>
public enum CommandPriority
{
    /// <summary>
    /// Normal priority command.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// High priority command (executed before Normal).
    /// </summary>
    High = 1,

    /// <summary>
    /// Emergency priority command (executed immediately).
    /// </summary>
    Emergency = 2
}

/// <summary>
/// Command execution status.
/// </summary>
public enum CommandStatus
{
    /// <summary>
    /// Command created but not yet executed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Command currently executing on robot.
    /// </summary>
    Executing = 1,

    /// <summary>
    /// Command completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Command execution failed.
    /// </summary>
    Failed = 3
}
