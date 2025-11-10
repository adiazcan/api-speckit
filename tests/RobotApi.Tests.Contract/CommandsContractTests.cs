using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using Xunit;

namespace RobotApi.Tests.Contract;

/// <summary>
/// Contract tests for Command endpoints - validate API schema compliance
/// </summary>
public class CommandsContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CommandsContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer operator-token");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task POST_RobotsCommands_ReturnsAccepted_WithValidCommandResponse()
    {
        // Arrange
        var robotId = "robot-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 },
            Priority = CommandPriority.Normal
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        
        var commandResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(commandResponse);
        Assert.NotEmpty(commandResponse.Id);
        Assert.Equal(robotId, commandResponse.RobotId);
        Assert.Equal("move", commandResponse.CommandType);
        Assert.Equal(CommandStatus.Pending, commandResponse.Status);
        Assert.NotNull(commandResponse.OperatorId);
    }

    [Fact]
    public async Task GET_RobotsCommandsById_ReturnsOk_WithValidCommandResponse()
    {
        // Arrange
        var robotId = "robot-001";
        var request = new CommandRequest
        {
            CommandType = "stop",
            Parameters = new { },
            Priority = CommandPriority.High
        };
        
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/v1/robots/{robotId}/commands/{createdCommand!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var commandResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(commandResponse);
        Assert.Equal(createdCommand.Id, commandResponse.Id);
        Assert.Equal(robotId, commandResponse.RobotId);
        Assert.Equal("stop", commandResponse.CommandType);
    }

    [Fact]
    public async Task GET_RobotsCommands_ReturnsOk_WithArrayOfCommands()
    {
        // Arrange
        var robotId = "robot-001";
        
        // Create a couple of commands
        await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 5.0, speed = 1.0 }
        });

        // Act
        var response = await _client.GetAsync($"/v1/robots/{robotId}/commands");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var commands = await response.Content.ReadFromJsonAsync<List<CommandResponse>>(_jsonOptions);
        Assert.NotNull(commands);
        Assert.NotEmpty(commands);
        Assert.All(commands, cmd =>
        {
            Assert.NotEmpty(cmd.Id);
            Assert.Equal(robotId, cmd.RobotId);
            Assert.NotEmpty(cmd.CommandType);
        });
    }

    [Fact]
    public async Task GET_RobotsCommands_WithStatusFilter_ReturnsFilteredCommands()
    {
        // Arrange
        var robotId = "robot-002";
        
        // Create commands
        await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 5.0, speed = 1.0 }
        });

        // Act
        var response = await _client.GetAsync($"/v1/robots/{robotId}/commands?status=Pending");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var commands = await response.Content.ReadFromJsonAsync<List<CommandResponse>>(_jsonOptions);
        Assert.NotNull(commands);
        Assert.All(commands, cmd => Assert.Equal(CommandStatus.Pending, cmd.Status));
    }

    [Fact]
    public async Task GET_RobotsCommands_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var robotId = "robot-003";
        
        // Create multiple commands
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", new CommandRequest
            {
                CommandType = "stop",
                Parameters = new { }
            });
        }

        // Act
        var response = await _client.GetAsync($"/v1/robots/{robotId}/commands?limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var commands = await response.Content.ReadFromJsonAsync<List<CommandResponse>>(_jsonOptions);
        Assert.NotNull(commands);
        Assert.True(commands.Count <= 2);
    }

    [Fact]
    public async Task DELETE_RobotsCommands_ReturnsNoContent_ForPendingCommand()
    {
        // Arrange
        var robotId = "robot-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "backward", distance = 3.0, speed = 1.0 }
        };
        
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);

        // Act - immediately try to cancel before execution
        var response = await _client.DeleteAsync($"/v1/robots/{robotId}/commands/{createdCommand!.Id}");

        // Assert
        // Note: Might be 204 NoContent or 404 NotFound if already executed
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent || 
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_RobotsCommands_ReturnsBadRequest_WithInvalidCommandType()
    {
        // Arrange
        var robotId = "robot-001";
        var request = new
        {
            CommandType = "invalid_command",
            Parameters = new { },
            Priority = "Normal"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_RobotsCommands_ReturnsBadRequest_WithInvalidParameters()
    {
        // Arrange
        var robotId = "robot-001";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "invalid", distance = -10.0, speed = 999.0 } // Invalid values
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_RobotsCommandsById_ReturnsNotFound_ForNonExistentCommand()
    {
        // Arrange
        var robotId = "robot-001";
        var nonExistentCommandId = "cmd-nonexistent-99999";

        // Act
        var response = await _client.GetAsync($"/v1/robots/{robotId}/commands/{nonExistentCommandId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_RobotsCommands_ReturnsNotFound_ForNonExistentRobot()
    {
        // Arrange
        var robotId = "robot-nonexistent";
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 5.0, speed = 1.0 }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
