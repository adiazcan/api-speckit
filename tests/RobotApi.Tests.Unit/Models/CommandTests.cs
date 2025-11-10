using FluentAssertions;
using RobotApi.Models;
using Xunit;

namespace RobotApi.Tests.Unit.Models;

/// <summary>
/// Unit tests for Command model and state machine
/// </summary>
public class CommandTests
{
    [Fact]
    public void Command_InitialState_ShouldBePending()
    {
        // Arrange & Act
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Assert
        command.Status.Should().Be(CommandStatus.Pending);
        command.CompletedAt.Should().BeNull();
        command.Result.Should().BeNull();
        command.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Command_CanTransitionFrom_Pending_To_Executing()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Act
        var canTransition = command.CanTransitionTo(CommandStatus.Executing);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void Command_CanTransitionFrom_Executing_To_Completed()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Executing,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Act
        var canTransition = command.CanTransitionTo(CommandStatus.Completed);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void Command_CanTransitionFrom_Executing_To_Failed()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Executing,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Act
        var canTransition = command.CanTransitionTo(CommandStatus.Failed);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void Command_CannotTransitionFrom_Completed()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Act & Assert
        command.CanTransitionTo(CommandStatus.Executing).Should().BeFalse();
        command.CanTransitionTo(CommandStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void Command_CannotTransitionFrom_Failed()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Failed,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Act & Assert
        command.CanTransitionTo(CommandStatus.Pending).Should().BeFalse();
        command.CanTransitionTo(CommandStatus.Executing).Should().BeFalse();
    }

    [Theory]
    [InlineData(CommandPriority.Normal)]
    [InlineData(CommandPriority.High)]
    [InlineData(CommandPriority.Emergency)]
    public void Command_SupportsAllPriorityLevels(CommandPriority priority)
    {
        // Arrange & Act
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "stop",
            Parameters = new { },
            Priority = priority,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Assert
        command.Priority.Should().Be(priority);
    }

    [Fact]
    public void Command_CompletedState_HasTimestampAndResult()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var completedAt = DateTime.UtcNow;
        
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Completed,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            OperatorId = "operator-001",
            Result = "Success"
        };

        // Assert
        command.CompletedAt.Should().NotBeNull();
        command.CompletedAt.Should().BeAfter(command.CreatedAt);
        command.Result.Should().Be("Success");
    }

    [Fact]
    public void Command_FailedState_HasErrorMessage()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Failed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            OperatorId = "operator-001",
            ErrorMessage = "Obstacle detected"
        };

        // Assert
        command.Status.Should().Be(CommandStatus.Failed);
        command.ErrorMessage.Should().NotBeNullOrEmpty();
        command.ErrorMessage.Should().Be("Obstacle detected");
    }

    [Theory]
    [InlineData("move")]
    [InlineData("rotate")]
    [InlineData("stop")]
    [InlineData("sensor_activate")]
    public void Command_SupportsExpectedCommandTypes(string commandType)
    {
        // Arrange & Act
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = commandType,
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Assert
        command.CommandType.Should().Be(commandType);
    }

    [Fact]
    public void Command_StoresParametersCorrectly()
    {
        // Arrange
        var parameters = new { direction = "forward", distance = 15.5, speed = 3.2 };
        
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = parameters,
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = "operator-001"
        };

        // Assert
        command.Parameters.Should().NotBeNull();
        command.Parameters.Should().BeEquivalentTo(parameters);
    }

    [Fact]
    public void Command_HasOperatorId()
    {
        // Arrange
        var operatorId = "operator-123";
        
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "stop",
            Parameters = new { },
            Priority = CommandPriority.High,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OperatorId = operatorId
        };

        // Assert
        command.OperatorId.Should().Be(operatorId);
    }

    [Fact]
    public void Command_DurationCanBeCalculated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddSeconds(-10);
        var completedAt = DateTime.UtcNow;
        
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Completed,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            OperatorId = "operator-001"
        };

        // Act
        var duration = command.CompletedAt!.Value - command.CreatedAt;

        // Assert
        duration.TotalSeconds.Should().BeGreaterThanOrEqualTo(10);
        duration.TotalSeconds.Should().BeLessThan(11);
    }
}
