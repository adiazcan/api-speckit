namespace RobotApi.Models.Dtos;

/// <summary>
/// Response DTO for robot capabilities (read-only view).
/// </summary>
public sealed class RobotCapabilitiesResponse
{
    /// <summary>
    /// List of supported command types.
    /// </summary>
    public required string[] Commands { get; init; }

    /// <summary>
    /// List of available sensors.
    /// </summary>
    public required string[] Sensors { get; init; }

    /// <summary>
    /// Maximum speed in m/s (optional).
    /// </summary>
    public double? MaxSpeed { get; init; }

    /// <summary>
    /// Maximum distance in meters (optional).
    /// </summary>
    public double? MaxDistance { get; init; }

    /// <summary>
    /// Creates a RobotCapabilitiesResponse from a RobotCapabilities entity.
    /// </summary>
    public static RobotCapabilitiesResponse FromCapabilities(RobotCapabilities capabilities)
    {
        return new RobotCapabilitiesResponse
        {
            Commands = capabilities.Commands.ToArray(), // Create a copy of the array
            Sensors = capabilities.Sensors.ToArray(), // Create a copy of the array
            MaxSpeed = capabilities.MaxSpeed,
            MaxDistance = capabilities.MaxDistance
        };
    }
}
