using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RobotApi.Extensions;
using RobotApi.Models.Dtos;
using RobotApi.Services;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for robot management.
/// </summary>
public static class RobotEndpoints
{
    public static RouteGroupBuilder MapRobotEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListRobots)
            .WithName("ListRobots")
            .WithSummary("List all robots")
            .RequireAuthorization("ViewerPolicy");

        group.MapPost("/", RegisterRobot)
            .WithName("RegisterRobot")
            .WithSummary("Register a new robot")
            .RequireAuthorization("AdminPolicy");

        group.MapGet("/{robotId}", GetRobot)
            .WithName("GetRobot")
            .WithSummary("Get robot by ID")
            .RequireAuthorization("ViewerPolicy");

        group.MapDelete("/{robotId}", DeregisterRobot)
            .WithName("DeregisterRobot")
            .WithSummary("Deregister a robot")
            .RequireAuthorization("AdminPolicy");

        return group;
    }

    private static async Task<IResult> ListRobots(IRobotService robotService)
    {
        var robots = await robotService.ListRobotsAsync();
        var responses = robots.Select(RobotResponse.FromRobot);
        return Results.Ok(responses);
    }

    private static async Task<IResult> RegisterRobot(
        [FromBody] RobotRegistrationRequest request,
        HttpContext context,
        IRobotService robotService,
        IValidator<RobotRegistrationRequest> validator)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var operatorId = context.User.GetOperatorId();
            var robot = await robotService.RegisterRobotAsync(request, operatorId);
            var response = RobotResponse.FromRobot(robot);
            return Results.Created($"/v1/robots/{robot.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetRobot(
        string robotId,
        IRobotService robotService)
    {
        var robot = await robotService.GetRobotAsync(robotId);

        if (robot == null)
        {
            return Results.NotFound(new { message = $"Robot with ID '{robotId}' not found" });
        }

        var response = RobotResponse.FromRobot(robot);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeregisterRobot(
        string robotId,
        IRobotService robotService)
    {
        var deleted = await robotService.DeregisterRobotAsync(robotId);

        if (!deleted)
        {
            return Results.NotFound(new { message = $"Robot with ID '{robotId}' not found" });
        }

        return Results.NoContent();
    }
}
