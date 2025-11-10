using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RobotApi.Extensions;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using RobotApi.Services;
using RobotApi.Validators;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for sending and managing robot commands.
/// </summary>
public static class CommandEndpoints
{
    /// <summary>
    /// Maps all command-related endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCommandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/robots/{robotId}/commands")
            .WithTags("Commands");

        group.MapPost("/", SendCommand)
            .WithName("SendCommand")
            .WithSummary("Send a command to a robot")
            .WithDescription("Submits a control command to a robot. Returns 202 Accepted with command ID. Requires Operator role or higher.")
            .Produces<CommandResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/{commandId}", GetCommand)
            .WithName("GetCommand")
            .WithSummary("Get command status")
            .WithDescription("Retrieves the current status and details of a specific command. Requires Operator role or higher.")
            .Produces<CommandResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListCommands)
            .WithName("ListCommands")
            .WithSummary("List robot commands")
            .WithDescription("Retrieves all commands for a robot with optional status filtering. Requires Operator role or higher.")
            .Produces<IEnumerable<CommandResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{commandId}", CancelCommand)
            .WithName("CancelCommand")
            .WithSummary("Cancel a pending command")
            .WithDescription("Cancels a pending command. Cannot cancel commands that are already executing or completed. Requires Operator role or higher.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> SendCommand(
        string robotId,
        [FromBody] CommandRequest request,
        HttpContext context,
        ICommandService commandService,
        IValidator<CommandRequest> requestValidator,
        ILogger<CommandRequest> logger)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Operator role minimum)
        if (!context.User.HasRole(OperatorRole.Operator))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Operator role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Validate request
        var validationResult = await requestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            return Results.Json(
                ProblemDetailsExtensions.BadRequest(
                    "Validation failed",
                    string.Join("; ", errors)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Validate command-specific parameters
        var parametersValidationResult = await ValidateCommandParameters(request);
        if (!parametersValidationResult.IsValid)
        {
            return Results.Json(
                ProblemDetailsExtensions.BadRequest(
                    "Invalid command parameters",
                    parametersValidationResult.ErrorMessage),
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var operatorId = context.User.GetOperatorId() 
                ?? throw new InvalidOperationException("Operator ID not found in claims");

            var command = await commandService.SendCommandAsync(robotId, request, operatorId);
            var response = CommandResponse.FromCommand(command);

            logger.LogInformation(
                "Command {CommandId} created for robot {RobotId} by operator {OperatorId}",
                command.Id, robotId, operatorId);

            return Results.AcceptedAtRoute(
                "GetCommand",
                new { robotId, commandId = command.Id },
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Resource not found", ex.Message),
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("offline"))
        {
            return Results.Json(
                ProblemDetailsExtensions.ServiceUnavailable("Robot unavailable", ex.Message),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Json(
                ProblemDetailsExtensions.BadRequest("Invalid request", ex.Message),
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> GetCommand(
        string robotId,
        string commandId,
        HttpContext context,
        ICommandService commandService)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Operator role minimum)
        if (!context.User.HasRole(OperatorRole.Operator))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Operator role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        var command = await commandService.GetCommandAsync(commandId);
        if (command == null)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Command not found", $"Command '{commandId}' not found"),
                statusCode: StatusCodes.Status404NotFound);
        }

        // Verify command belongs to the specified robot
        if (command.RobotId != robotId)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Command not found", $"Command '{commandId}' not found for robot '{robotId}'"),
                statusCode: StatusCodes.Status404NotFound);
        }

        var response = CommandResponse.FromCommand(command);
        return Results.Ok(response);
    }

    private static async Task<IResult> ListCommands(
        string robotId,
        HttpContext context,
        ICommandService commandService,
        [FromQuery] string? status = null)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Operator role minimum)
        if (!context.User.HasRole(OperatorRole.Operator))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Operator role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Parse status filter if provided
        CommandStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<CommandStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
            else
            {
                return Results.Json(
                    ProblemDetailsExtensions.BadRequest(
                        "Invalid status parameter",
                        $"Status must be one of: {string.Join(", ", Enum.GetNames<CommandStatus>())}"),
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        var commands = await commandService.ListCommandsAsync(robotId, statusFilter);
        var response = commands.Select(CommandResponse.FromCommand).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> CancelCommand(
        string robotId,
        string commandId,
        HttpContext context,
        ICommandService commandService)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Operator role minimum)
        if (!context.User.HasRole(OperatorRole.Operator))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Operator role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Get command to verify it belongs to this robot
        var command = await commandService.GetCommandAsync(commandId);
        if (command == null)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Command not found", $"Command '{commandId}' not found"),
                statusCode: StatusCodes.Status404NotFound);
        }

        if (command.RobotId != robotId)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Command not found", $"Command '{commandId}' not found for robot '{robotId}'"),
                statusCode: StatusCodes.Status404NotFound);
        }

        var cancelled = await commandService.CancelCommandAsync(commandId);
        if (!cancelled)
        {
            return Results.Json(
                ProblemDetailsExtensions.BadRequest(
                    "Cannot cancel command",
                    $"Command '{commandId}' cannot be cancelled (status: {command.Status})"),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.NoContent();
    }

    private static async Task<(bool IsValid, string ErrorMessage)> ValidateCommandParameters(CommandRequest request)
    {
        try
        {
            var commandType = request.CommandType.ToLowerInvariant();

            // Convert JsonElement to specific parameter type for validation
            if (request.Parameters is JsonElement jsonElement)
            {
                switch (commandType)
                {
                    case "move":
                        var moveParams = JsonSerializer.Deserialize<MoveCommandParameters>(jsonElement.GetRawText());
                        if (moveParams == null)
                        {
                            return (false, "Move command requires parameters");
                        }
                        var moveValidator = new MoveCommandParametersValidator();
                        var moveResult = await moveValidator.ValidateAsync(moveParams);
                        if (!moveResult.IsValid)
                        {
                            return (false, string.Join("; ", moveResult.Errors.Select(e => e.ErrorMessage)));
                        }
                        break;

                    case "rotate":
                        var rotateParams = JsonSerializer.Deserialize<RotateCommandParameters>(jsonElement.GetRawText());
                        if (rotateParams == null)
                        {
                            return (false, "Rotate command requires parameters");
                        }
                        var rotateValidator = new RotateCommandParametersValidator();
                        var rotateResult = await rotateValidator.ValidateAsync(rotateParams);
                        if (!rotateResult.IsValid)
                        {
                            return (false, string.Join("; ", rotateResult.Errors.Select(e => e.ErrorMessage)));
                        }
                        break;

                    case "sensor_activate":
                        var sensorParams = JsonSerializer.Deserialize<SensorActivateCommandParameters>(jsonElement.GetRawText());
                        if (sensorParams == null)
                        {
                            return (false, "Sensor activate command requires parameters");
                        }
                        var sensorValidator = new SensorActivateCommandParametersValidator();
                        var sensorResult = await sensorValidator.ValidateAsync(sensorParams);
                        if (!sensorResult.IsValid)
                        {
                            return (false, string.Join("; ", sensorResult.Errors.Select(e => e.ErrorMessage)));
                        }
                        break;

                    case "stop":
                        // Stop command has no parameters to validate
                        break;
                }
            }

            return (true, string.Empty);
        }
        catch (JsonException ex)
        {
            return (false, $"Invalid JSON parameters: {ex.Message}");
        }
    }
}
