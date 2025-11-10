using RobotApi.Data;
using RobotApi.Models;

namespace RobotApi.Services;

/// <summary>
/// Background service that simulates command execution by transitioning command states.
/// In a real implementation, this would communicate with actual robots.
/// </summary>
public sealed class CommandExecutorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandExecutorService> _logger;
    private readonly TimeSpan _executionCheckInterval = TimeSpan.FromSeconds(2);

    public CommandExecutorService(
        IServiceProvider serviceProvider,
        ILogger<CommandExecutorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Command Executor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingCommandsAsync(stoppingToken);
                await ProcessExecutingCommandsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in command executor service");
            }

            await Task.Delay(_executionCheckInterval, stoppingToken);
        }

        _logger.LogInformation("Command Executor Service stopped");
    }

    private async Task ProcessPendingCommandsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandStore = scope.ServiceProvider.GetRequiredService<CommandStore>();
        var robotStore = scope.ServiceProvider.GetRequiredService<RobotStore>();

        var pendingCommands = await commandStore.GetPendingCommandsAsync();

        foreach (var command in pendingCommands)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            // Check if robot is still online
            var robot = await robotStore.GetByIdAsync(command.RobotId);
            if (robot == null || robot.ConnectionStatus == ConnectionStatus.Offline)
            {
                command.Status = CommandStatus.Failed;
                command.CompletedAt = DateTime.UtcNow;
                command.ErrorMessage = "Robot is offline";
                await commandStore.UpdateAsync(command.Id, command);
                _logger.LogWarning("Command {CommandId} failed - robot {RobotId} is offline",
                    command.Id, command.RobotId);
                continue;
            }

            // Transition to Executing
            command.Status = CommandStatus.Executing;
            command.ExecutedAt = DateTime.UtcNow;
            await commandStore.UpdateAsync(command.Id, command);

            _logger.LogInformation("Command {CommandId} started executing on robot {RobotId}",
                command.Id, command.RobotId);
        }
    }

    private async Task ProcessExecutingCommandsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandStore = scope.ServiceProvider.GetRequiredService<CommandStore>();
        var robotStore = scope.ServiceProvider.GetRequiredService<RobotStore>();

        var allCommands = await commandStore.GetAllAsync();
        var executingCommands = allCommands
            .Where(c => c.Status == CommandStatus.Executing)
            .ToList();

        foreach (var command in executingCommands)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            // Simulate execution time based on command type
            var executionDuration = GetSimulatedExecutionDuration(command);
            var elapsed = DateTime.UtcNow - (command.ExecutedAt ?? DateTime.UtcNow);

            if (elapsed >= executionDuration)
            {
                // Check if robot is still online
                var robot = await robotStore.GetByIdAsync(command.RobotId);
                if (robot == null || robot.ConnectionStatus == ConnectionStatus.Offline)
                {
                    command.Status = CommandStatus.Failed;
                    command.CompletedAt = DateTime.UtcNow;
                    command.ErrorMessage = "Robot disconnected during execution";
                    await commandStore.UpdateAsync(command.Id, command);
                    _logger.LogWarning("Command {CommandId} failed - robot {RobotId} disconnected",
                        command.Id, command.RobotId);
                    continue;
                }

                // Simulate 90% success rate
                var random = new Random();
                var isSuccess = random.NextDouble() > 0.1;

                if (isSuccess)
                {
                    command.Status = CommandStatus.Completed;
                    command.CompletedAt = DateTime.UtcNow;
                    command.Result = new
                    {
                        success = true,
                        message = $"Command '{command.CommandType}' completed successfully",
                        executionTime = elapsed.TotalSeconds
                    };

                    _logger.LogInformation("Command {CommandId} completed successfully on robot {RobotId}",
                        command.Id, command.RobotId);
                }
                else
                {
                    command.Status = CommandStatus.Failed;
                    command.CompletedAt = DateTime.UtcNow;
                    command.ErrorMessage = "Simulated execution error - obstacle detected";

                    _logger.LogWarning("Command {CommandId} failed on robot {RobotId}",
                        command.Id, command.RobotId);
                }

                await commandStore.UpdateAsync(command.Id, command);
            }
        }
    }

    private TimeSpan GetSimulatedExecutionDuration(Command command)
    {
        // Simulate different execution times based on command type
        return command.CommandType switch
        {
            "move" => TimeSpan.FromSeconds(5),
            "rotate" => TimeSpan.FromSeconds(3),
            "stop" => TimeSpan.FromSeconds(1),
            "sensor_activate" => TimeSpan.FromSeconds(2),
            _ => TimeSpan.FromSeconds(3)
        };
    }
}
