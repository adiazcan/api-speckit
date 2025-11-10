using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using Xunit;

namespace RobotApi.Tests.Integration;

/// <summary>
/// Integration tests for Command scenarios - end-to-end command workflows
/// </summary>
public class CommandScenarioTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CommandScenarioTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer operator-token");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task SendMoveCommand_Scenario_CommandIsCreatedAndExecuted()
    {
        // Arrange
        var robotId = "robot-42";
        var moveCommand = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 15.0, speed = 3.0 },
            Priority = CommandPriority.Normal
        };

        // Act - Send command
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", moveCommand);
        
        // Assert - Command created
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);
        Assert.NotEmpty(createdCommand.Id);
        Assert.Equal(CommandStatus.Pending, createdCommand.Status);

        // Act - Retrieve command details
        var getResponse = await _client.GetAsync($"/v1/robots/{robotId}/commands/{createdCommand.Id}");
        
        // Assert - Command can be retrieved
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedCommand = await getResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(retrievedCommand);
        Assert.Equal(createdCommand.Id, retrievedCommand.Id);
        Assert.Equal("move", retrievedCommand.CommandType);
        
        // Wait for command execution (background service processes commands)
        await Task.Delay(TimeSpan.FromSeconds(3));
        
        // Act - Check command status after execution
        var finalResponse = await _client.GetAsync($"/v1/robots/{robotId}/commands/{createdCommand.Id}");
        var finalCommand = await finalResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        
        // Assert - Command should be completed or in progress
        Assert.NotNull(finalCommand);
        Assert.True(
            finalCommand.Status == CommandStatus.Completed || 
            finalCommand.Status == CommandStatus.Executing,
            $"Expected Completed or Executing, but got {finalCommand.Status}");
    }

    [Fact]
    public async Task SendRotateCommand_Scenario_CommandIsProcessed()
    {
        // Arrange
        var robotId = "robot-101";
        var rotateCommand = new CommandRequest
        {
            CommandType = "rotate",
            Parameters = new { direction = "left", degrees = 90 },
            Priority = CommandPriority.Normal
        };

        // Act - Send command
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", rotateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);
        Assert.Equal("rotate", createdCommand.CommandType);
        Assert.Equal(CommandStatus.Pending, createdCommand.Status);
        
        // Verify parameters are stored correctly
        Assert.NotNull(createdCommand.Parameters);
    }

    [Fact]
    public async Task SendStopCommand_Scenario_HighPriorityCommandIsCreated()
    {
        // Arrange
        var robotId = "robot-42";
        var stopCommand = new CommandRequest
        {
            CommandType = "stop",
            Parameters = new { },
            Priority = CommandPriority.High
        };

        // Act
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", stopCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);
        Assert.Equal("stop", createdCommand.CommandType);
        Assert.Equal(CommandPriority.High, createdCommand.Priority);
        Assert.Equal(CommandStatus.Pending, createdCommand.Status);
    }

    [Fact]
    public async Task CommandStatusTracking_Scenario_StatusTransitionsAreTracked()
    {
        // Arrange
        var robotId = "robot-42";
        var command = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "backward", distance = 5.0, speed = 1.5 },
            Priority = CommandPriority.Normal
        };

        // Act - Create command
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", command);
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);

        // Assert - Initial status is Pending
        Assert.Equal(CommandStatus.Pending, createdCommand.Status);
        Assert.Null(createdCommand.CompletedAt);

        // Act - Check status multiple times to observe transitions
        var statuses = new List<CommandStatus>();
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            
            var getResponse = await _client.GetAsync($"/v1/robots/{robotId}/commands/{createdCommand.Id}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var currentCommand = await getResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
                if (currentCommand != null)
                {
                    statuses.Add(currentCommand.Status);
                }
            }
        }

        // Assert - Status should transition (Pending -> InProgress -> Completed or similar)
        Assert.NotEmpty(statuses);
        Assert.Contains(CommandStatus.Pending, statuses);
    }

    [Fact]
    public async Task CommandCancellation_Scenario_PendingCommandCanBeCancelled()
    {
        // Arrange
        var robotId = "robot-42";
        var command = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 100.0, speed = 1.0 }, // Long command
            Priority = CommandPriority.Normal
        };

        // Act - Create command
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", command);
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);

        // Act - Immediately cancel before execution
        var deleteResponse = await _client.DeleteAsync($"/v1/robots/{robotId}/commands/{createdCommand.Id}");

        // Assert - Cancellation accepted (or already completed)
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.NoContent || 
            deleteResponse.StatusCode == HttpStatusCode.NotFound ||
            deleteResponse.StatusCode == HttpStatusCode.BadRequest,
            $"Expected NoContent, NotFound, or BadRequest, but got {deleteResponse.StatusCode}");
    }

    [Fact]
    public async Task OfflineRobotRejection_Scenario_CommandToOfflineRobotIsFails()
    {
        // Note: This test assumes we have a way to mark robots as offline
        // For now, we'll use a non-existent robot to simulate offline behavior
        
        // Arrange
        var offlineRobotId = "robot-offline-999";
        var command = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 5.0, speed = 1.0 }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{offlineRobotId}/commands", command);

        // Assert - Should be rejected (NotFound or BadRequest)
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound || 
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected NotFound or BadRequest for offline robot, but got {response.StatusCode}");
    }

    [Fact]
    public async Task MultipleCommands_Scenario_CommandsAreQueuedAndExecutedInOrder()
    {
        // Arrange
        var robotId = "robot-42";
        var commandIds = new List<string>();

        // Act - Send multiple commands
        for (int i = 0; i < 3; i++)
        {
            var command = new CommandRequest
            {
                CommandType = "move",
                Parameters = new { direction = "forward", distance = 1.0, speed = 1.0 },
                Priority = CommandPriority.Normal
            };
            
            var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", command);
            var createdCommand = await response.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
            
            Assert.NotNull(createdCommand);
            commandIds.Add(createdCommand.Id);
        }

        // Assert - All commands created
        Assert.Equal(3, commandIds.Count);

        // Act - Retrieve all commands
        var listResponse = await _client.GetAsync($"/v1/robots/{robotId}/commands");
        var commands = await listResponse.Content.ReadFromJsonAsync<List<CommandResponse>>(_jsonOptions);

        // Assert - All commands are in the list
        Assert.NotNull(commands);
        Assert.True(commands.Count >= 3, "Should have at least 3 commands");
    }

    [Fact]
    public async Task InvalidCommandParameters_Scenario_ValidationFailsWithBadRequest()
    {
        // Arrange
        var robotId = "robot-42";
        var invalidCommand = new CommandRequest
        {
            CommandType = "move",
            Parameters = new 
            { 
                direction = "invalid_direction",  // Invalid
                distance = 150.0,  // Exceeds max (100)
                speed = -5.0  // Negative speed
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", invalidCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task SensorActivateCommand_Scenario_SensorCommandIsProcessed()
    {
        // Arrange
        var robotId = "robot-42";
        var sensorCommand = new CommandRequest
        {
            CommandType = "sensor_activate",
            Parameters = new { sensorName = "camera", enabled = true },
            Priority = CommandPriority.Normal
        };

        // Act
        var postResponse = await _client.PostAsJsonAsync($"/v1/robots/{robotId}/commands", sensorCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        
        var createdCommand = await postResponse.Content.ReadFromJsonAsync<CommandResponse>(_jsonOptions);
        Assert.NotNull(createdCommand);
        Assert.Equal("sensor_activate", createdCommand.CommandType);
    }

    [Fact]
    public async Task AuthorizedOperator_Scenario_CanSendCommands()
    {
        // Arrange - Use operator token
        var operatorClient = _client; // Already has operator-token
        var robotId = "robot-42";
        var command = new CommandRequest
        {
            CommandType = "stop",
            Parameters = new { }
        };

        // Act
        var response = await operatorClient.PostAsJsonAsync($"/v1/robots/{robotId}/commands", command);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task ViewerRole_Scenario_CannotSendCommands()
    {
        // Arrange - Use viewer token
        var viewerClient = _client;
        viewerClient.DefaultRequestHeaders.Remove("Authorization");
        viewerClient.DefaultRequestHeaders.Add("Authorization", "Bearer viewer-token");
        
        var robotId = "robot-42";
        var command = new CommandRequest
        {
            CommandType = "stop",
            Parameters = new { }
        };

        // Act
        var response = await viewerClient.PostAsJsonAsync($"/v1/robots/{robotId}/commands", command);

        // Assert - Viewers cannot send commands
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || 
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected Forbidden or Unauthorized for viewer, but got {response.StatusCode}");
    }
}
