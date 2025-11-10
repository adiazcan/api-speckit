using FluentValidation.TestHelper;
using RobotApi.Models.Dtos;
using RobotApi.Validators;
using Xunit;

namespace RobotApi.Tests.Unit.Validators;

/// <summary>
/// Unit tests for command validation rules
/// </summary>
public class CommandValidatorTests
{
    private readonly CommandRequestValidator _validator;

    public CommandValidatorTests()
    {
        _validator = new CommandRequestValidator();
    }

    [Fact]
    public void Validate_WithValidMoveCommand_PassesValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = new { direction = "forward", distance = 10.0, speed = 2.0 }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidRotateCommand_PassesValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "rotate",
            Parameters = new { direction = "left", degrees = 90 }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidStopCommand_PassesValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "stop",
            Parameters = new { }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidSensorActivateCommand_PassesValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "sensor_activate",
            Parameters = new { sensorName = "camera", enabled = true }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCommandType_FailsValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "",
            Parameters = new { }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommandType);
    }

    [Fact]
    public void Validate_WithInvalidCommandType_FailsValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "invalid_command",
            Parameters = new { }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommandType);
    }

    [Fact]
    public void Validate_WithNullParameters_FailsValidation()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandType = "move",
            Parameters = null!
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parameters);
    }
}

/// <summary>
/// Unit tests for MoveCommandParametersValidator
/// </summary>
public class MoveCommandParametersValidatorTests
{
    private readonly MoveCommandParametersValidator _validator;

    public MoveCommandParametersValidatorTests()
    {
        _validator = new MoveCommandParametersValidator();
    }

    [Theory]
    [InlineData("forward")]
    [InlineData("backward")]
    [InlineData("left")]
    [InlineData("right")]
    public void Validate_WithValidDirection_PassesValidation(string direction)
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = direction,
            Distance = 10.0,
            Speed = 2.0
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Direction);
    }

    [Fact]
    public void Validate_WithInvalidDirection_FailsValidation()
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = "diagonal",
            Distance = 10.0,
            Speed = 2.0
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Direction);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    public void Validate_WithValidDistance_PassesValidation(double distance)
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = "forward",
            Distance = distance,
            Speed = 2.0
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Distance);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(100.1)]
    [InlineData(200.0)]
    public void Validate_WithInvalidDistance_FailsValidation(double distance)
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = "forward",
            Distance = distance,
            Speed = 2.0
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Distance);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void Validate_WithValidSpeed_PassesValidation(double speed)
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = "forward",
            Distance = 10.0,
            Speed = speed
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Speed);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Validate_WithInvalidSpeed_FailsValidation(double speed)
    {
        // Arrange
        var parameters = new MoveCommandParameters
        {
            Direction = "forward",
            Distance = 10.0,
            Speed = speed
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Speed);
    }
}

/// <summary>
/// Unit tests for RotateCommandParametersValidator
/// </summary>
public class RotateCommandParametersValidatorTests
{
    private readonly RotateCommandParametersValidator _validator;

    public RotateCommandParametersValidatorTests()
    {
        _validator = new RotateCommandParametersValidator();
    }

    [Theory]
    [InlineData("left")]
    [InlineData("right")]
    public void Validate_WithValidDirection_PassesValidation(string direction)
    {
        // Arrange
        var parameters = new RotateCommandParameters
        {
            Direction = direction,
            Degrees = 90
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Direction);
    }

    [Fact]
    public void Validate_WithInvalidDirection_FailsValidation()
    {
        // Arrange
        var parameters = new RotateCommandParameters
        {
            Direction = "up",
            Degrees = 90
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Direction);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(360)]
    public void Validate_WithValidDegrees_PassesValidation(int degrees)
    {
        // Arrange
        var parameters = new RotateCommandParameters
        {
            Direction = "left",
            Degrees = degrees
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Degrees);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(361)]
    [InlineData(500)]
    public void Validate_WithInvalidDegrees_FailsValidation(int degrees)
    {
        // Arrange
        var parameters = new RotateCommandParameters
        {
            Direction = "left",
            Degrees = degrees
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Degrees);
    }
}

/// <summary>
/// Unit tests for SensorActivateCommandParametersValidator
/// </summary>
public class SensorActivateCommandParametersValidatorTests
{
    private readonly SensorActivateCommandParametersValidator _validator;

    public SensorActivateCommandParametersValidatorTests()
    {
        _validator = new SensorActivateCommandParametersValidator();
    }

    [Theory]
    [InlineData("camera")]
    [InlineData("lidar")]
    [InlineData("temperature")]
    public void Validate_WithValidSensorName_PassesValidation(string sensorName)
    {
        // Arrange
        var parameters = new SensorActivateCommandParameters
        {
            SensorName = sensorName,
            Enabled = true
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SensorName);
    }

    [Fact]
    public void Validate_WithEmptySensorName_FailsValidation()
    {
        // Arrange
        var parameters = new SensorActivateCommandParameters
        {
            SensorName = "",
            Enabled = true
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SensorName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WithValidEnabled_PassesValidation(bool enabled)
    {
        // Arrange
        var parameters = new SensorActivateCommandParameters
        {
            SensorName = "camera",
            Enabled = enabled
        };

        // Act
        var result = _validator.TestValidate(parameters);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
