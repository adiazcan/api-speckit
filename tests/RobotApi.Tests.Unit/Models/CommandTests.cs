using FluentAssertions;
using RobotApi.Models;
using Xunit;

namespace RobotApi.Tests.Unit.Models;

/// <summary>
/// Unit tests for Command state machine transitions
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
            CreatedBy = "operator-001"
        };

        // Assert
        command.Status.Should().Be(CommandStatus.Pending);
        command.CompletedAt.Should().BeNull();
        command.Result.Should().BeNull();
        command.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Command_TransitionToPending_ToInProgress_IsValid()
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
            CreatedBy = "operator-001"
        };

        // Act
        command.Status = CommandStatus.InProgress;

        // Assert
        command.Status.Should().Be(CommandStatus.InProgress);
    }

    [Fact]
    public void Command_TransitionToInProgress_ToCompleted_IsValid()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        // Act
        command.Status = CommandStatus.Completed;
        command.CompletedAt = DateTime.UtcNow;
        command.Result = "Success";

        // Assert
        command.Status.Should().Be(CommandStatus.Completed);
        command.CompletedAt.Should().NotBeNull();
        command.Result.Should().Be("Success");
    }

    [Fact]
    public void Command_TransitionToInProgress_ToFailed_IsValid()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "move",
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        // Act
        command.Status = CommandStatus.Failed;
        command.CompletedAt = DateTime.UtcNow;
        command.ErrorMessage = "Robot connection lost";

        // Assert
        command.Status.Should().Be(CommandStatus.Failed);
        command.CompletedAt.Should().NotBeNull();
        command.ErrorMessage.Should().Be("Robot connection lost");
    }

    [Fact]
    public void Command_TransitionToPending_ToCancelled_IsValid()
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
            CreatedBy = "operator-001"
        };

        // Act
        command.Status = CommandStatus.Cancelled;
        command.CompletedAt = DateTime.UtcNow;

        // Assert
        command.Status.Should().Be(CommandStatus.Cancelled);
        command.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Command_WithHighPriority_HasCorrectPriorityLevel()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "stop",
            Parameters = new { },
            Priority = CommandPriority.High,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        // Assert
        command.Priority.Should().Be(CommandPriority.High);
    }

    [Fact]
    public void Command_WithLowPriority_HasCorrectPriorityLevel()
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = "sensor_activate",
            Parameters = new { sensorName = "camera", enabled = true },
            Priority = CommandPriority.Low,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        // Assert
        command.Priority.Should().Be(CommandPriority.Low);
    }

    [Fact]
    public void Command_CompletedState_ShouldHaveCompletedTimestamp()
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
            CreatedBy = "operator-001",
            Result = "Success"
        };

        // Assert
        command.CompletedAt.Should().NotBeNull();
        command.CompletedAt.Should().BeAfter(command.CreatedAt);
    }

    [Fact]
    public void Command_FailedState_ShouldHaveErrorMessage()
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
            CreatedBy = "operator-001",
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
    public void Command_SupportedCommandTypes_AreValid(string commandType)
    {
        // Arrange
        var command = new Command
        {
            Id = "cmd-001",
            RobotId = "robot-001",
            CommandType = commandType,
            Parameters = new { },
            Priority = CommandPriority.Normal,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "operator-001"
        };

        // Assert
        command.CommandType.Should().Be(commandType);
    }

    [Fact]
    public void Command_WithParameters_StoresParametersCorrectly()
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
            CreatedBy = "operator-001"
        };

        // Assert
        command.Parameters.Should().NotBeNull();
        command.Parameters.Should().BeEquivalentTo(parameters);
    }

    [Fact]
    public void Command_CreatedByOperator_HasOperatorId()
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
            CreatedBy = operatorId
        };

        // Assert
        command.CreatedBy.Should().Be(operatorId);
    }

    [Fact]
    public void Command_Duration_CanBeCalculated()
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
            CreatedBy = "operator-001"
        };

        // Act
        var duration = command.CompletedAt!.Value - command.CreatedAt;

        // Assert
        duration.TotalSeconds.Should().BeGreaterThanOrEqualTo(10);
        duration.TotalSeconds.Should().BeLessThan(11);
    }
}
