using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for robot management operations.
/// </summary>
public interface IRobotService
{
    /// <summary>
    /// Registers a new robot in the system.
    /// </summary>
    Task<Robot> RegisterRobotAsync(RobotRegistrationRequest request);

    /// <summary>
    /// Gets a robot by ID.
    /// </summary>
    Task<Robot?> GetRobotAsync(string robotId);

    /// <summary>
    /// Lists all robots in the system.
    /// </summary>
    Task<IEnumerable<Robot>> ListRobotsAsync();

    /// <summary>
    /// Deregisters a robot from the system.
    /// </summary>
    Task<bool> DeregisterRobotAsync(string robotId);
}
