using Microsoft.AspNetCore.Mvc;
using RobotApi.Models;
using RobotApi.Models.Dtos;
using RobotApi.Services;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for event retrieval.
/// </summary>
public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListEvents)
            .WithName("ListEvents")
            .WithSummary("List events with optional filtering")
            .RequireAuthorization("OperatorPolicy");

        return group;
    }

    private static async Task<IResult> ListEvents(
        IEventService eventService,
        [FromQuery] string? robotId = null,
        [FromQuery] string? eventType = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        // Parse severity if provided
        EventSeverity? severityEnum = null;
        if (!string.IsNullOrEmpty(severity))
        {
            if (Enum.TryParse<EventSeverity>(severity, true, out var parsedSeverity))
            {
                severityEnum = parsedSeverity;
            }
            else
            {
                return Results.BadRequest(new
                {
                    message = $"Invalid severity value '{severity}'. Valid values are: Info, Warning, Error, Critical"
                });
            }
        }

        var events = await eventService.ListEventsAsync(
            robotId,
            eventType,
            severityEnum,
            startTime,
            endTime);

        var responses = events.Select(EventResponse.FromEvent);

        return Results.Ok(responses);
    }
}
