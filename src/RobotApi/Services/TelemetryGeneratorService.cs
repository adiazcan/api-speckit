using RobotApi.Data;
using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Background service that simulates telemetry data generation for online robots.
/// In a real implementation, this would receive data from actual robots via IoT protocols.
/// </summary>
public sealed class TelemetryGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelemetryGeneratorService> _logger;
    private readonly TimeSpan _generationInterval = TimeSpan.FromSeconds(2);
    private readonly Random _random = new();

    public TelemetryGeneratorService(
        IServiceProvider serviceProvider,
        ILogger<TelemetryGeneratorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry Generator Service started");

        // Wait a bit for other services to initialize
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateTelemetryForOnlineRobotsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in telemetry generator service");
            }

            await Task.Delay(_generationInterval, stoppingToken);
        }

        _logger.LogInformation("Telemetry Generator Service stopped");
    }

    private async Task GenerateTelemetryForOnlineRobotsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var robotStore = scope.ServiceProvider.GetRequiredService<RobotStore>();
        var telemetryStore = scope.ServiceProvider.GetRequiredService<TelemetryStore>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var robots = await robotStore.GetAllAsync();
        var onlineRobots = robots.Where(r => r.ConnectionStatus == ConnectionStatus.Online).ToList();

        foreach (var robot in onlineRobots)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var telemetry = GenerateTelemetrySnapshot(robot);
            await telemetryStore.AddAsync(telemetry);

            // Check for battery_low event
            if (telemetry.BatteryLevel < 20)
            {
                await eventService.CreateEventAsync(
                    robot.Id,
                    "battery_low",
                    EventSeverity.Warning,
                    $"Robot battery level is low: {telemetry.BatteryLevel}%",
                    new Dictionary<string, object>
                    {
                        { "batteryLevel", telemetry.BatteryLevel },
                        { "timestamp", telemetry.Timestamp }
                    });
            }

            // Check for sensor_threshold event (high temperature)
            if (telemetry.Temperature > 50)
            {
                await eventService.CreateEventAsync(
                    robot.Id,
                    "sensor_threshold",
                    EventSeverity.Error,
                    $"Robot temperature is critically high: {telemetry.Temperature}°C",
                    new Dictionary<string, object>
                    {
                        { "temperature", telemetry.Temperature },
                        { "threshold", 50 },
                        { "sensor", "temperature" },
                        { "timestamp", telemetry.Timestamp }
                    });
            }
            else if (telemetry.Temperature > 45)
            {
                await eventService.CreateEventAsync(
                    robot.Id,
                    "sensor_threshold",
                    EventSeverity.Warning,
                    $"Robot temperature is high: {telemetry.Temperature}°C",
                    new Dictionary<string, object>
                    {
                        { "temperature", telemetry.Temperature },
                        { "threshold", 45 },
                        { "sensor", "temperature" },
                        { "timestamp", telemetry.Timestamp }
                    });
            }

            // Update robot's LastSeenAt
            await robotStore.UpdateLastSeenAsync(robot.Id);
        }

        if (onlineRobots.Any())
        {
            _logger.LogDebug("Generated telemetry for {Count} online robots", onlineRobots.Count);
        }
    }

    private Telemetry GenerateTelemetrySnapshot(Robot robot)
    {
        // Get previous telemetry to simulate realistic changes
        var previousTelemetry = _serviceProvider.CreateScope()
            .ServiceProvider.GetRequiredService<TelemetryStore>()
            .GetLatestByRobotIdAsync(robot.Id).Result;

        // Generate position (simulate movement)
        var position = GeneratePosition(previousTelemetry);

        // Generate orientation
        var orientation = new Orientation
        {
            Pitch = _random.NextDouble() * 5 - 2.5, // -2.5 to 2.5 degrees
            Roll = _random.NextDouble() * 5 - 2.5,
            Yaw = _random.NextDouble() * 360 // 0 to 360 degrees
        };

        // Generate speed
        var speed = robot.Capabilities?.MaxSpeed != null
            ? _random.NextDouble() * Math.Min(robot.Capabilities.MaxSpeed.Value, 5.0)
            : _random.NextDouble() * 3.0;

        // Generate battery level (decreases slowly over time)
        var batteryLevel = previousTelemetry != null
            ? Math.Max(10, previousTelemetry.BatteryLevel - _random.Next(0, 2))
            : _random.Next(50, 100);

        // Generate temperature (normal operating range)
        var temperature = 35 + _random.NextDouble() * 7; // 35-42°C

        // Generate sensor readings
        var sensorReadings = new Dictionary<string, double>();
        if (robot.Capabilities?.Sensors != null)
        {
            foreach (var sensor in robot.Capabilities.Sensors)
            {
                sensorReadings[sensor] = sensor switch
                {
                    "battery" => batteryLevel,
                    "temperature" => temperature,
                    "proximity" => _random.NextDouble() * 10, // 0-10 meters
                    "vibration" => _random.NextDouble() * 0.1, // 0-0.1 g
                    "humidity" => 30 + _random.NextDouble() * 40, // 30-70%
                    _ => _random.NextDouble() * 100
                };
            }
        }

        return new Telemetry
        {
            Id = Guid.NewGuid().ToString(),
            RobotId = robot.Id,
            Timestamp = DateTime.UtcNow,
            Position = position,
            Orientation = orientation,
            Speed = Math.Round(speed, 2),
            BatteryLevel = batteryLevel,
            Temperature = Math.Round(temperature, 1),
            SensorReadings = sensorReadings
        };
    }

    private Position GeneratePosition(Telemetry? previousTelemetry)
    {
        // Use local coordinates for warehouse robots
        if (previousTelemetry?.Position != null && previousTelemetry.Position.Type == "local")
        {
            // Simulate small movements
            var deltaX = (_random.NextDouble() - 0.5) * 2; // -1 to 1 meter
            var deltaY = (_random.NextDouble() - 0.5) * 2;

            return new Position
            {
                Type = "local",
                X = (previousTelemetry.Position.X ?? 0) + deltaX,
                Y = (previousTelemetry.Position.Y ?? 0) + deltaY,
                Z = 0
            };
        }

        // Default: start at origin with local coordinates
        return new Position
        {
            Type = "local",
            X = _random.NextDouble() * 100, // 0-100 meters
            Y = _random.NextDouble() * 100,
            Z = 0
        };
    }
}
