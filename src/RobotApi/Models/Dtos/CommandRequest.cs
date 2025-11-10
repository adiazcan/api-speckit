namespace RobotApi.Models.Dtos;

/// <summary>
/// Request DTO for sending a command to a robot.
/// </summary>
public sealed class CommandRequest
{
    /// <summary>
    /// Gets or sets the command type (e.g., "move", "rotate", "stop", "sensor_activate").
    /// </summary>
    public required string CommandType { get; set; }

    /// <summary>
    /// Gets or sets the command-specific parameters.
    /// </summary>
    public required object Parameters { get; set; }

    /// <summary>
    /// Gets or sets the command priority level (defaults to Normal if not specified).
    /// </summary>
    public CommandPriority Priority { get; set; } = CommandPriority.Normal;
}

/// <summary>
/// Parameters for move command.
/// </summary>
public sealed class MoveCommandParameters
{
    /// <summary>
    /// Gets or sets the movement direction (forward, backward, left, right).
    /// </summary>
    public required string Direction { get; set; }

    /// <summary>
    /// Gets or sets the distance to move in meters (0-100).
    /// </summary>
    public required double Distance { get; set; }

    /// <summary>
    /// Gets or sets the movement speed in m/s.
    /// </summary>
    public required double Speed { get; set; }
}

/// <summary>
/// Parameters for rotate command.
/// </summary>
public sealed class RotateCommandParameters
{
    /// <summary>
    /// Gets or sets the rotation direction (left, right).
    /// </summary>
    public required string Direction { get; set; }

    /// <summary>
    /// Gets or sets the degrees to rotate (0-360).
    /// </summary>
    public required int Degrees { get; set; }
}

/// <summary>
/// Parameters for stop command (no parameters required).
/// </summary>
public sealed class StopCommandParameters
{
    // No parameters needed for stop command
}

/// <summary>
/// Parameters for sensor_activate command.
/// </summary>
public sealed class SensorActivateCommandParameters
{
    /// <summary>
    /// Gets or sets the sensor name to activate/deactivate.
    /// </summary>
    public required string SensorName { get; set; }

    /// <summary>
    /// Gets or sets whether to enable or disable the sensor.
    /// </summary>
    public required bool Enabled { get; set; }
}
