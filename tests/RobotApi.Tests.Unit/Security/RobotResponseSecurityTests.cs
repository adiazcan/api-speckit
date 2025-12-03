using RobotApi.Models;
using RobotApi.Models.Dtos;

namespace RobotApi.Tests.Unit.Security;

/// <summary>
/// Tests to ensure sensitive internal fields are not exposed in API responses.
/// </summary>
public class RobotResponseSecurityTests
{
    [Fact]
    public void RobotResponse_ShouldNotExposeRegisteredByOperatorId()
    {
        // Arrange
        var robot = new Robot
        {
            Id = "test-robot",
            Name = "Test Robot",
            ModelType = "Test-1000",
            FirmwareVersion = "1.0.0",
            ConnectionStatus = ConnectionStatus.Online,
            RegisteredAt = DateTime.UtcNow,
            RegisteredByOperatorId = "operator-secret", // Internal tracking field
            Capabilities = new RobotCapabilities
            {
                Commands = new[] { "move", "stop" },
                Sensors = new[] { "battery" },
                MaxSpeed = 5.0,
                MaxDistance = 100.0
            }
        };

        // Act
        var response = RobotResponse.FromRobot(robot);

        // Assert
        var responseType = response.GetType();
        var properties = responseType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Verify that RegisteredByOperatorId is NOT in the response
        Assert.DoesNotContain("RegisteredByOperatorId", propertyNames);
        Assert.DoesNotContain("registeredByOperatorId", propertyNames);
        Assert.DoesNotContain("Owner", propertyNames);
        Assert.DoesNotContain("owner", propertyNames);
    }

    [Fact]
    public void RobotResponse_ShouldOnlyExposeApprovedFields()
    {
        // Arrange
        var robot = new Robot
        {
            Id = "test-robot",
            Name = "Test Robot",
            ModelType = "Test-1000",
            FirmwareVersion = "1.0.0",
            ConnectionStatus = ConnectionStatus.Online,
            RegisteredAt = DateTime.UtcNow,
            RegisteredByOperatorId = "operator-secret",
            Capabilities = new RobotCapabilities
            {
                Commands = new[] { "move" },
                Sensors = new[] { "battery" },
            }
        };

        // Act
        var response = RobotResponse.FromRobot(robot);

        // Assert - only these fields should be present
        var responseType = response.GetType();
        var properties = responseType.GetProperties().Select(p => p.Name).ToHashSet();

        var expectedProperties = new HashSet<string>
        {
            "Id",
            "Name",
            "ModelType",
            "FirmwareVersion",
            "ConnectionStatus",
            "Capabilities",
            "RegisteredAt",
            "LastSeenAt"
        };

        Assert.Equal(expectedProperties, properties);
    }

    [Fact]
    public void RobotCapabilitiesResponse_ShouldBeImmutable()
    {
        // Arrange
        var capabilities = new RobotCapabilities
        {
            Commands = new[] { "move", "stop" },
            Sensors = new[] { "battery", "temperature" },
            MaxSpeed = 5.0,
            MaxDistance = 100.0
        };

        // Act
        var response = RobotCapabilitiesResponse.FromCapabilities(capabilities);

        // Assert - response should have init-only properties
        var responseType = response.GetType();
        foreach (var property in responseType.GetProperties())
        {
            // All properties should have init accessor (SetMethod should be init-only)
            var setMethod = property.SetMethod;
            if (setMethod != null)
            {
                // Check that the setter has the IsInitOnly required modifier
                var isInitOnly = setMethod.ReturnParameter.GetRequiredCustomModifiers()
                    .Any(t => t.Name == "IsExternalInit");
                Assert.True(isInitOnly, $"Property {property.Name} should be init-only");
            }
        }
    }

    [Fact]
    public void RobotCapabilitiesResponse_ShouldCreateDefensiveCopyOfArrays()
    {
        // Arrange
        var originalCommands = new[] { "move", "stop" };
        var capabilities = new RobotCapabilities
        {
            Commands = originalCommands,
            Sensors = new[] { "battery" },
        };

        // Act
        var response = RobotCapabilitiesResponse.FromCapabilities(capabilities);

        // Assert - modifying original should not affect response
        originalCommands[0] = "modified";
        Assert.NotEqual("modified", response.Commands[0]);
        Assert.Equal("move", response.Commands[0]);
    }
}
