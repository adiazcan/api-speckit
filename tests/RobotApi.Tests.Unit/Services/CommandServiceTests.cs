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
    private readonly Mock<IDataStore<Command>> _mockCommandStore;
    private readonly Mock<IDataStore<Robot>> _mockRobotStore;
    private readonly Mock<ILogger<CommandService>> _mockLogger;
    private readonly CommandService _commandService;

    public CommandServiceTests()
    {
        _mockCommandStore = new Mock<IDataStore<Command>>();
        _mockRobotStore = new Mock<IDataStore<Robot>>();
        _mockLogger = new Mock<ILogger<CommandService>>();
        
        _commandService = new CommandService(
            _mockCommandStore.Object,
            _mockRobotStore.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendCommandAsync_WithValidRequest_CreatesCommandSuccessfully()
    {
        // Arrange
        var robotId = "robot-001";
        var operatorId = "operator-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 },
            Priority = CommandPriority.Normal
        };

        var robot = new Robot
        {
            Id = robotId,
            Name = "Test Robot",
            Model = "TR-100",
            FirmwareVersion = "1.0.0",
            Status = RobotStatus.Active,
            Location = new Location { X = 0, Y = 0, Z = 0 },
            Capabilities = new RobotCapabilities
            {
                Commands = new[] { "move", "rotate", "stop" },
                Sensors = new[] { "camera" },
                MaxSpeed = 10.0,
                MaxDistance = 100.0
            },
            RegisteredAt = DateTime.UtcNow
        };

        _mockRobotStore.Setup(s => s.GetByIdAsync(robotId))
            .ReturnsAsync(robot);

        _mockCommandStore.Setup(s => s.AddAsync(It.IsAny<Command>()))
            .ReturnsAsync((Command cmd) => cmd);

        // Act
        var result = await _commandService.SendCommandAsync(robotId, request, operatorId);

        // Assert
        result.Should().NotBeNull();
        result.RobotId.Should().Be(robotId);
        result.CommandType.Should().Be("move");
        result.Status.Should().Be(CommandStatus.Pending);
        result.CreatedBy.Should().Be(operatorId);
        result.Priority.Should().Be(CommandPriority.Normal);

        _mockCommandStore.Verify(s => s.AddAsync(It.Is<Command>(c =>
            c.RobotId == robotId &&
            c.CommandType == "move" &&
            c.Status == CommandStatus.Pending &&
            c.CreatedBy == operatorId
        )), Times.Once);
    }

    [Fact]
    public async Task SendCommandAsync_WithNonExistentRobot_ThrowsException()
    {
        // Arrange
        var robotId = "robot-nonexistent";
        var operatorId = "operator-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 }
        };

        _mockRobotStore.Setup(s => s.GetByIdAsync(robotId))
            .ReturnsAsync((Robot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _commandService.SendCommandAsync(robotId, request, operatorId));

        _mockCommandStore.Verify(s => s.AddAsync(It.IsAny<Command>()), Times.Never);
    }

    [Fact]
    public async Task GetCommandAsync_WithExistingCommand_ReturnsCommand()
    {
        // Arrange
        var commandId = "cmd-001";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 },
            Status = CommandStatus.Pending,
            Priority = CommandPriority.Normal,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        _mockCommandStore.Setup(s => s.GetByIdAsync(commandId))
            .ReturnsAsync(command);

        // Act
        var result = await _commandService.GetCommandAsync(commandId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(commandId);
        result.CommandType.Should().Be("move");
        result.Status.Should().Be(CommandStatus.Pending);
    }

    [Fact]
    public async Task GetCommandAsync_WithNonExistentCommand_ReturnsNull()
    {
        // Arrange
        var commandId = "cmd-nonexistent";

        _mockCommandStore.Setup(s => s.GetByIdAsync(commandId))
            .ReturnsAsync((Command?)null);

        // Act
        var result = await _commandService.GetCommandAsync(commandId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListCommandsAsync_ReturnsAllCommandsForRobot()
    {
        // Arrange
        var robotId = "robot-001";
        var commands = new List<Command>
        {
            new Command
            {
                Id = "cmd-001",
                RobotId = robotId,
                CommandType = "move",
                Parameters = new { },
                Status = CommandStatus.Completed,
                Priority = CommandPriority.Normal,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "operator-001"
            },
            new Command
            {
                Id = "cmd-002",
                RobotId = robotId,
                CommandType = "stop",
                Parameters = new { },
                Status = CommandStatus.Pending,
                Priority = CommandPriority.High,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "operator-001"
            }
        };

        _mockCommandStore.Setup(s => s.ListAsync())
            .ReturnsAsync(commands);

        // Act
        var result = await _commandService.ListCommandsAsync(robotId, null, null);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(cmd => cmd.RobotId.Should().Be(robotId));
    }

    [Fact]
    public async Task ListCommandsAsync_WithStatusFilter_ReturnsFilteredCommands()
    {
        // Arrange
        var robotId = "robot-001";
        var commands = new List<Command>
        {
            new Command
            {
                Id = "cmd-001",
                RobotId = robotId,
                CommandType = "move",
                Parameters = new { },
                Status = CommandStatus.Completed,
                Priority = CommandPriority.Normal,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "operator-001"
            },
            new Command
            {
                Id = "cmd-002",
                RobotId = robotId,
                CommandType = "stop",
                Parameters = new { },
                Status = CommandStatus.Pending,
                Priority = CommandPriority.High,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "operator-001"
            }
        };

        _mockCommandStore.Setup(s => s.ListAsync())
            .ReturnsAsync(commands);

        // Act
        var result = await _commandService.ListCommandsAsync(robotId, CommandStatus.Pending, null);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(CommandStatus.Pending);
    }

    [Fact]
    public async Task ListCommandsAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var robotId = "robot-001";
        var commands = new List<Command>
        {
            new Command { Id = "cmd-001", RobotId = robotId, CommandType = "move", Parameters = new { }, Status = CommandStatus.Pending, Priority = CommandPriority.Normal, CreatedAt = DateTime.UtcNow, CreatedBy = "op" },
            new Command { Id = "cmd-002", RobotId = robotId, CommandType = "stop", Parameters = new { }, Status = CommandStatus.Pending, Priority = CommandPriority.Normal, CreatedAt = DateTime.UtcNow, CreatedBy = "op" },
            new Command { Id = "cmd-003", RobotId = robotId, CommandType = "move", Parameters = new { }, Status = CommandStatus.Pending, Priority = CommandPriority.Normal, CreatedAt = DateTime.UtcNow, CreatedBy = "op" }
        };

        _mockCommandStore.Setup(s => s.ListAsync())
            .ReturnsAsync(commands);

        // Act
        var result = await _commandService.ListCommandsAsync(robotId, null, 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CancelCommandAsync_WithPendingCommand_CancelsSuccessfully()
    {
        // Arrange
        var commandId = "cmd-001";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Status = CommandStatus.Pending,
            Priority = CommandPriority.Normal,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        _mockCommandStore.Setup(s => s.GetByIdAsync(commandId))
            .ReturnsAsync(command);

        _mockCommandStore.Setup(s => s.UpdateAsync(commandId, It.IsAny<Command>()))
            .ReturnsAsync(true);

        // Act
        var result = await _commandService.CancelCommandAsync(commandId);

        // Assert
        result.Should().BeTrue();

        _mockCommandStore.Verify(s => s.UpdateAsync(commandId, It.Is<Command>(c =>
            c.Status == CommandStatus.Cancelled
        )), Times.Once);
    }

    [Fact]
    public async Task CancelCommandAsync_WithCompletedCommand_ReturnsFalse()
    {
        // Arrange
        var commandId = "cmd-001";
        var command = new Command
        {
            Id = commandId,
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Status = CommandStatus.Completed,
            Priority = CommandPriority.Normal,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001",
            CompletedAt = DateTime.UtcNow
        };

        _mockCommandStore.Setup(s => s.GetByIdAsync(commandId))
            .ReturnsAsync(command);

        // Act
        var result = await _commandService.CancelCommandAsync(commandId);

        // Assert
        result.Should().BeFalse();

        _mockCommandStore.Verify(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<Command>()), Times.Never);
    }

    [Fact]
    public async Task CancelCommandAsync_WithNonExistentCommand_ReturnsFalse()
    {
        // Arrange
        var commandId = "cmd-nonexistent";

        _mockCommandStore.Setup(s => s.GetByIdAsync(commandId))
            .ReturnsAsync((Command?)null);

        // Act
        var result = await _commandService.CancelCommandAsync(commandId);

        // Assert
        result.Should().BeFalse();
    }
}
