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
    /// <param name="request">Robot registration details</param>
    /// <param name="operatorId">ID of the operator registering the robot (optional)</param>
    Task<Robot> RegisterRobotAsync(RobotRegistrationRequest request, string? operatorId = null);

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
