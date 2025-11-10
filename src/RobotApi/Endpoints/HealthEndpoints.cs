namespace RobotApi.Endpoints;

/// <summary>
/// Health check endpoints for monitoring.
/// </summary>
public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetHealth)
            .WithName("GetHealth")
            .WithSummary("Get API health status")
            .AllowAnonymous();

        return group;
    }

    private static IResult GetHealth()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            service = "IoT Robot Control & Telemetry API"
        };

        return Results.Ok(health);
    }
}
