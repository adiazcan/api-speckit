using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Services;

/// <summary>
/// Service for robot management operations.
/// </summary>
public sealed class RobotService : IRobotService
{
    private readonly RobotStore _robotStore;
    private readonly ILogger<RobotService> _logger;

    public RobotService(RobotStore robotStore, ILogger<RobotService> logger)
    {
        _robotStore = robotStore;
        _logger = logger;
    }

    public async Task<Robot> RegisterRobotAsync(RobotRegistrationRequest request)
    {
        // Check if robot with same name already exists
        var existingRobots = await _robotStore.GetAllAsync();
        if (existingRobots.Any(r => r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A robot with name '{request.Name}' already exists");
        }

        var robot = new Robot
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ModelType = request.ModelType,
            FirmwareVersion = request.FirmwareVersion,
            ConnectionStatus = ConnectionStatus.Offline,
            Capabilities = request.Capabilities,
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = null
        };

        await _robotStore.AddAsync(robot);

        _logger.LogInformation(
            "Registered new robot {RobotId} with name '{RobotName}' and model type '{ModelType}'",
            robot.Id, robot.Name, robot.ModelType);

        return robot;
    }

    public Task<Robot?> GetRobotAsync(string robotId)
    {
        return _robotStore.GetByIdAsync(robotId);
    }

    public Task<IEnumerable<Robot>> ListRobotsAsync()
    {
        return _robotStore.GetAllAsync();
    }

    public async Task<bool> DeregisterRobotAsync(string robotId)
    {
        var robot = await _robotStore.GetByIdAsync(robotId);
        if (robot == null)
        {
            return false;
        }

        var deleted = await _robotStore.DeleteAsync(robotId);

        if (deleted)
        {
            _logger.LogInformation(
                "Deregistered robot {RobotId} with name '{RobotName}'",
                robotId, robot.Name);
        }

        return deleted;
    }
}
