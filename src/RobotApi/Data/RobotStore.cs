using System.Collections.Concurrent;
using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// In-memory data store for robots with seeded test data.
/// </summary>
public class RobotStore : IRobotStore
{
    private readonly ConcurrentDictionary<string, Robot> _robots = new();
    private readonly ILogger<RobotStore> _logger;

    public RobotStore(ILogger<RobotStore> logger)
    {
        _logger = logger;
        SeedData();
    }

    private void SeedData()
    {
        var robots = new[]
        {
            new Robot
            {
                Id = "robot-42",
                Name = "Warehouse Robot Alpha",
                ModelType = "RoboX-3000",
                FirmwareVersion = "2.5.1",
                ConnectionStatus = ConnectionStatus.Online,
                LastSeenAt = DateTime.UtcNow,
                RegisteredAt = DateTime.UtcNow.AddDays(-60),
                Capabilities = new RobotCapabilities
                {
                    Commands = new string[] { "move", "rotate", "stop", "sensor_activate" },
                    Sensors = new string[] { "battery", "temperature", "position", "proximity" },
                    MaxSpeed = 5.0,
                    MaxDistance = 100.0
                }
            },
            new Robot
            {
                Id = "robot-101",
                Name = "Manufacturing Bot Beta",
                ModelType = "RoboX-2000",
                FirmwareVersion = "1.8.3",
                ConnectionStatus = ConnectionStatus.Online,
                LastSeenAt = DateTime.UtcNow.AddMinutes(-2),
                RegisteredAt = DateTime.UtcNow.AddDays(-45),
                Capabilities = new RobotCapabilities
                {
                    Commands = new string[] { "move", "rotate", "stop" },
                    Sensors = new string[] { "battery", "temperature", "position" },
                    MaxSpeed = 3.0,
                    MaxDistance = 50.0
                }
            },
            new Robot
            {
                Id = "robot-247",
                Name = "Logistics Carrier Gamma",
                ModelType = "RoboX-4000",
                FirmwareVersion = "3.1.0",
                ConnectionStatus = ConnectionStatus.Offline,
                LastSeenAt = DateTime.UtcNow.AddHours(-2),
                RegisteredAt = DateTime.UtcNow.AddDays(-30),
                Capabilities = new RobotCapabilities
                {
                    Commands = new string[] { "move", "rotate", "stop", "sensor_activate" },
                    Sensors = new string[] { "battery", "temperature", "position", "proximity", "vibration" },
                    MaxSpeed = 7.0,
                    MaxDistance = 150.0
                }
            },
            new Robot
            {
                Id = "robot-333",
                Name = "Inspection Drone Delta",
                ModelType = "RoboX-5000",
                FirmwareVersion = "2.2.0",
                ConnectionStatus = ConnectionStatus.Online,
                LastSeenAt = DateTime.UtcNow.AddSeconds(-30),
                RegisteredAt = DateTime.UtcNow.AddDays(-15),
                Capabilities = new RobotCapabilities
                {
                    Commands = new string[] { "move", "rotate", "stop", "sensor_activate" },
                    Sensors = new string[] { "battery", "temperature", "position", "proximity", "humidity" },
                    MaxSpeed = 10.0,
                    MaxDistance = 200.0
                }
            },
            new Robot
            {
                Id = "robot-555",
                Name = "Assembly Line Bot Epsilon",
                ModelType = "RoboX-1500",
                FirmwareVersion = "1.5.2",
                ConnectionStatus = ConnectionStatus.Offline,
                LastSeenAt = DateTime.UtcNow.AddDays(-1),
                RegisteredAt = DateTime.UtcNow.AddDays(-90),
                Capabilities = new RobotCapabilities
                {
                    Commands = new string[] { "move", "stop" },
                    Sensors = new string[] { "battery", "temperature" },
                    MaxSpeed = 2.0,
                    MaxDistance = 25.0
                }
            }
        };

        foreach (var robot in robots)
        {
            _robots.TryAdd(robot.Id, robot);
        }

        _logger.LogInformation("Seeded {Count} robots", robots.Length);
    }

    public Task<Robot?> GetByIdAsync(string id)
    {
        _robots.TryGetValue(id, out var robot);
        return Task.FromResult(robot);
    }

    public Task<IEnumerable<Robot>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Robot>>(_robots.Values.ToList());
    }

    public Task<Robot> AddAsync(Robot entity)
    {
        if (_robots.TryAdd(entity.Id, entity))
        {
            _logger.LogInformation("Added robot: {RobotId}", entity.Id);
            return Task.FromResult(entity);
        }

        throw new InvalidOperationException($"Robot with ID {entity.Id} already exists");
    }

    public Task<Robot?> UpdateAsync(string id, Robot entity)
    {
        if (_robots.TryGetValue(id, out var existing))
        {
            entity.Id = id; // Ensure ID doesn't change
            _robots[id] = entity;
            _logger.LogInformation("Updated robot: {RobotId}", id);
            return Task.FromResult<Robot?>(entity);
        }

        return Task.FromResult<Robot?>(null);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _robots.TryRemove(id, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted robot: {RobotId}", id);
        }
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_robots.ContainsKey(id));
    }

    /// <summary>
    /// Updates the last seen timestamp for a robot (used by telemetry updates).
    /// </summary>
    public Task UpdateLastSeenAsync(string id)
    {
        if (_robots.TryGetValue(id, out var robot))
        {
            robot.LastSeenAt = DateTime.UtcNow;
            robot.ConnectionStatus = ConnectionStatus.Online;
        }
        return Task.CompletedTask;
    }
}
