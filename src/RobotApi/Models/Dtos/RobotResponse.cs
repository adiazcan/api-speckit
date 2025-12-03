namespace RobotApi.Models.Dtos;

/// <summary>
/// Response containing robot details.
/// </summary>
public sealed class RobotResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ModelType { get; init; }
    public required string FirmwareVersion { get; init; }
    public required string ConnectionStatus { get; init; }
    public required RobotCapabilitiesResponse Capabilities { get; init; }
    public required DateTime RegisteredAt { get; init; }
    public DateTime? LastSeenAt { get; init; }

    /// <summary>
    /// Maps a Robot entity to a RobotResponse DTO.
    /// </summary>
    public static RobotResponse FromRobot(Robot robot)
    {
        return new RobotResponse
        {
            Id = robot.Id,
            Name = robot.Name,
            ModelType = robot.ModelType,
            FirmwareVersion = robot.FirmwareVersion,
            ConnectionStatus = robot.ConnectionStatus.ToString(),
            Capabilities = RobotCapabilitiesResponse.FromCapabilities(robot.Capabilities),
            RegisteredAt = robot.RegisteredAt,
            LastSeenAt = robot.LastSeenAt
        };
    }
}
