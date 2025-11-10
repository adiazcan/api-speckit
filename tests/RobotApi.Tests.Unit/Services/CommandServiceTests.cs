using FluentAssertions;
using Moq;
using RobotApi.Data;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using RobotApi.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace RobotApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for CommandService
/// </summary>
public class CommandServiceTests
{
    private readonly Mock<ICommandStore> _mockCommandStore;
    private readonly Mock<IRobotStore> _mockRobotStore;
    private readonly Mock<IEventService> _mockEventService;
    private readonly Mock<ILogger<CommandService>> _mockLogger;
    private readonly CommandService _commandService;

    public CommandServiceTests()
    {
        _mockCommandStore = new Mock<ICommandStore>();
        _mockRobotStore = new Mock<IRobotStore>();
        _mockEventService = new Mock<IEventService>();
        _mockLogger = new Mock<ILogger<CommandService>>();
        
        _commandService = new CommandService(
            _mockCommandStore.Object,
            _mockRobotStore.Object,
            _mockEventService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task SendCommandAsync_WithValidRobot_CreatesCommand()
    {
        // Arrange
        var robotId = "robot-42";
        var operatorId = "operator-001";
        var robot = new Robot
        {
            Id = robotId,
            Name = "Test Robot",
            ModelType = "RoboX-3000",
            FirmwareVersion = "1.0.0",
            ConnectionStatus = ConnectionStatus.Online,
            RegisteredAt = DateTime.UtcNow,
            Capabilities = new RobotCapabilities
            {
                Commands = new[] { "move", "rotate", "stop" },
                Sensors = new[] { "battery", "temperature" },
                MaxSpeed = 5.0
            }
        };
        
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 },
            Priority = CommandPriority.Normal
        };

        _mockRobotStore.Setup(x => x.GetByIdAsync(robotId))
            .ReturnsAsync(robot);
        _mockCommandStore.Setup(x => x.AddAsync(It.IsAny<Command>()))
            .ReturnsAsync((Command cmd) => cmd);

        // Act
        var result = await _commandService.SendCommandAsync(robotId, request, operatorId);

        // Assert
        result.Should().NotBeNull();
        result.RobotId.Should().Be(robotId);
        result.CommandType.Should().Be("move");
        result.Status.Should().Be(CommandStatus.Pending);
        result.OperatorId.Should().Be(operatorId);
        
        _mockCommandStore.Verify(x => x.AddAsync(It.IsAny<Command>()), Times.Once);
    }

    [Fact]
    public async Task SendCommandAsync_WithNonExistentRobot_ThrowsException()
    {
        // Arrange
        var robotId = "non-existent";
        var operatorId = "operator-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal
        };

        _mockRobotStore.Setup(x => x.GetByIdAsync(robotId))
            .ReturnsAsync((Robot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commandService.SendCommandAsync(robotId, request, operatorId));
    }

    [Fact]
    public async Task GetCommandAsync_ReturnsCommand()
    {
        // Arrange
        var commandId = "cmd-123";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-42",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        _mockCommandStore.Setup(x => x.GetByIdAsync(commandId))
            .ReturnsAsync(command);

        // Act
        var result = await _commandService.GetCommandAsync(commandId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(commandId);
    }

    [Fact]
    public async Task ListCommandsAsync_ReturnsAllCommandsForRobot()
    {
        // Arrange
        var robotId = "robot-42";
        var commands = new List<Command>
        {
            new Command
            {
                Id = "cmd-1",
                RobotId = robotId,
                CommandType = "move",
                Parameters = new { },
                Priority = CommandPriority.Normal,
                Status = CommandStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OperatorId = "operator-001"
            },
            new Command
            {
                Id = "cmd-2",
                RobotId = robotId,
                CommandType = "stop",
                Parameters = new { },
                Priority = CommandPriority.High,
                Status = CommandStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                OperatorId = "operator-001"
            }
        };

        _mockCommandStore.Setup(x => x.GetByRobotIdAndStatusAsync(robotId, null))
            .ReturnsAsync(commands);

        // Act
        var result = await _commandService.ListCommandsAsync(robotId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.RobotId.Should().Be(robotId));
    }

    [Fact]
    public async Task ListCommandsAsync_WithStatusFilter_ReturnsFilteredCommands()
    {
        // Arrange
        var robotId = "robot-42";
        var pendingCommands = new List<Command>
        {
            new Command
            {
                Id = "cmd-1",
                RobotId = robotId,
                CommandType = "move",
                Parameters = new { },
                Priority = CommandPriority.Normal,
                Status = CommandStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OperatorId = "operator-001"
            }
        };

        _mockCommandStore.Setup(x => x.GetByRobotIdAndStatusAsync(robotId, CommandStatus.Pending))
            .ReturnsAsync(pendingCommands);

        // Act
        var result = await _commandService.ListCommandsAsync(robotId, CommandStatus.Pending);

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(c => c.Status.Should().Be(CommandStatus.Pending));
    }

    [Fact]
    public async Task CancelCommandAsync_WithPendingCommand_ReturnsTrue()
    {
        // Arrange
        var commandId = "cmd-123";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-42",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        _mockCommandStore.Setup(x => x.GetByIdAsync(commandId))
            .ReturnsAsync(command);
        _mockCommandStore.Setup(x => x.UpdateAsync(commandId, It.IsAny<Command>()))
            .ReturnsAsync(command);

        // Act
        var result = await _commandService.CancelCommandAsync(commandId);

        // Assert
        result.Should().BeTrue();
        _mockCommandStore.Verify(x => x.UpdateAsync(commandId, It.Is<Command>(c => 
            c.Status == CommandStatus.Failed && 
            c.ErrorMessage == "Command cancelled by operator")), Times.Once);
    }

    [Fact]
    public async Task CancelCommandAsync_WithCompletedCommand_ReturnsFalse()
    {
        // Arrange
        var commandId = "cmd-123";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-42",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        _mockCommandStore.Setup(x => x.GetByIdAsync(commandId))
            .ReturnsAsync(command);

        // Act
        var result = await _commandService.CancelCommandAsync(commandId);

        // Assert
        result.Should().BeFalse();
        _mockCommandStore.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Command>()), Times.Never);
    }
}
