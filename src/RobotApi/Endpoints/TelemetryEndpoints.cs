using FluentValidation;
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

        group.MapGet("/history", GetTelemetryHistory)
            .WithName("GetTelemetryHistory")
            .WithSummary("Get historical telemetry")
            .WithDescription("Retrieves historical telemetry snapshots with time range filtering and pagination. Maximum 90-day time range. Requires Viewer role or higher.")
            .Produces<TelemetryHistoryResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/export", ExportTelemetry)
            .WithName("ExportTelemetry")
            .WithSummary("Export telemetry to CSV")
            .WithDescription("Exports telemetry data to CSV format for the specified time range. Maximum 90-day time range. Requires Viewer role or higher.")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

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

    private static async Task<IResult> GetTelemetryHistory(
        string robotId,
        HttpContext context,
        ITelemetryService telemetryService,
        IValidator<TelemetryHistoryRequest> validator,
        ILogger<ITelemetryService> logger,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] string[]? telemetryTypes = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Viewer role minimum)
        if (!context.User.HasRole(OperatorRole.Viewer))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Viewer role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Build request
        var request = new TelemetryHistoryRequest
        {
            StartTime = startTime,
            EndTime = endTime,
            TelemetryTypes = telemetryTypes,
            Page = page,
            PageSize = pageSize
        };

        // Validate request
        var validationResult = await validator.ValidateAsync(request);
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

        try
        {
            var response = await telemetryService.GetHistoricalTelemetryAsync(robotId, request);
            
            logger.LogInformation(
                "Historical telemetry retrieved for robot {RobotId} by user {Username}: page {Page}/{TotalPages}, {RecordCount} records",
                robotId,
                context.User.GetOperatorUsername(),
                response.Page,
                response.TotalPages,
                response.Data.Length);

            return Results.Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Robot not found", ex.Message),
                statusCode: StatusCodes.Status404NotFound);
        }
    }

    private static async Task<IResult> ExportTelemetry(
        string robotId,
        HttpContext context,
        ITelemetryExportService exportService,
        IValidator<TelemetryHistoryRequest> validator,
        ILogger<ITelemetryExportService> logger,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] string[]? telemetryTypes = null,
        [FromQuery] string format = "csv")
    {
        // Check authentication
        if (!context.User.IsAuthenticated())
        {
            return Results.Json(
                ProblemDetailsExtensions.Unauthorized("Authentication required"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Check authorization (Viewer role minimum)
        if (!context.User.HasRole(OperatorRole.Viewer))
        {
            return Results.Json(
                ProblemDetailsExtensions.Forbidden("Requires Viewer role or higher"),
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Validate format
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(
                ProblemDetailsExtensions.BadRequest("Invalid format", "Only 'csv' format is supported"),
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Build validation request (reuse validation logic for time range and 90-day limit)
        var request = new TelemetryHistoryRequest
        {
            StartTime = startTime,
            EndTime = endTime,
            TelemetryTypes = telemetryTypes,
            Page = 1,
            PageSize = 100 // Not used for export, but needed for validation
        };

        var validationResult = await validator.ValidateAsync(request);
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

        try
        {
            var csvData = await exportService.ExportToCsvAsync(robotId, startTime, endTime, telemetryTypes);
            
            var filename = $"telemetry_{robotId}_{startTime:yyyyMMdd}_{endTime:yyyyMMdd}.csv";
            
            logger.LogInformation(
                "Telemetry exported to CSV for robot {RobotId} by user {Username}: {FileName}",
                robotId,
                context.User.GetOperatorUsername(),
                filename);

            context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
            return Results.Content(csvData, "text/csv");
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Json(
                ProblemDetailsExtensions.NotFound("Robot not found", ex.Message),
                statusCode: StatusCodes.Status404NotFound);
        }
    }
}
