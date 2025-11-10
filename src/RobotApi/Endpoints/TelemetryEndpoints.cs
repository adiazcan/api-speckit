using Microsoft.AspNetCore.Mvc;
using RobotApi.Extensions;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using RobotApi.Services;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for retrieving robot telemetry data.
/// </summary>
public static class TelemetryEndpoints
{
    /// <summary>
    /// Maps all telemetry-related endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/robots/{robotId}/telemetry")
            .WithTags("Telemetry");

        group.MapGet("/", GetCurrentTelemetry)
            .WithName("GetCurrentTelemetry")
            .WithSummary("Get current telemetry")
            .WithDescription("Retrieves the most recent telemetry snapshot for a robot. Requires Viewer role or higher.")
            .Produces<TelemetryResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> GetCurrentTelemetry(
        string robotId,
        HttpContext context,
        ITelemetryService telemetryService,
        ILogger<ITelemetryService> logger)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Viewer role minimum - all authenticated users can view telemetry)
        if (!context.User.HasRole(OperatorRole.Viewer))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Viewer role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        try
        {
            var telemetry = await telemetryService.GetCurrentTelemetryAsync(robotId);
            
            if (telemetry == null)
            {
                return Results.Json(
                    ProblemDetailsExtensions.ServiceUnavailable(
                        "No telemetry available",
                        $"No telemetry data available for robot '{robotId}'"),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var response = TelemetryResponse.FromTelemetry(telemetry);
            
            logger.LogInformation(
                "Telemetry retrieved for robot {RobotId} by user {Username} (freshness: {Freshness:F1}s)",
                robotId,
                context.User.GetOperatorUsername(),
                response.DataFreshness);

            return Results.Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Robot not found", ex.Message),
                statusCode: StatusCodes.Status404NotFound);
        }
    }
}
