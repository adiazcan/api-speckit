namespace RobotApi.Endpoints;

/// <summary>
/// API documentation endpoints.
/// </summary>
public static class DocumentationEndpoints
{
    public static RouteGroupBuilder MapDocumentationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/openapi.json", GetOpenApiSpec)
            .WithName("GetOpenApiSpec")
            .WithSummary("Get OpenAPI specification")
            .AllowAnonymous();

        return group;
    }

    private static IResult GetOpenApiSpec()
    {
        var paths = new Dictionary<string, object>
        {
            ["/v1/robots/{robotId}/commands"] = new
            {
                post = new
                {
                    summary = "Send command to robot",
                    tags = new[] { "Commands" },
                    security = new[] { new { BearerAuth = new string[] { } } },
                    parameters = new[]
                    {
                        new { name = "robotId", @in = "path", required = true, schema = new { type = "string" } }
                    }
                },
                get = new
                {
                    summary = "List robot commands",
                    tags = new[] { "Commands" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/robots/{robotId}/commands/{commandId}"] = new
            {
                get = new
                {
                    summary = "Get command details",
                    tags = new[] { "Commands" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/robots/{robotId}/telemetry"] = new
            {
                get = new
                {
                    summary = "Get latest telemetry",
                    tags = new[] { "Telemetry" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/robots/{robotId}/telemetry/history"] = new
            {
                get = new
                {
                    summary = "Get historical telemetry",
                    tags = new[] { "Telemetry" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/robots/{robotId}/telemetry/export"] = new
            {
                get = new
                {
                    summary = "Export telemetry as CSV",
                    tags = new[] { "Telemetry" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/subscriptions"] = new
            {
                post = new
                {
                    summary = "Create event subscription",
                    tags = new[] { "Subscriptions" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                },
                get = new
                {
                    summary = "List subscriptions",
                    tags = new[] { "Subscriptions" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/events"] = new
            {
                get = new
                {
                    summary = "List events",
                    tags = new[] { "Events" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/robots"] = new
            {
                get = new
                {
                    summary = "List all robots",
                    tags = new[] { "Robots" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                },
                post = new
                {
                    summary = "Register new robot",
                    tags = new[] { "Robots" },
                    security = new[] { new { BearerAuth = new string[] { } } }
                }
            },
            ["/v1/health"] = new
            {
                get = new
                {
                    summary = "Get API health status",
                    tags = new[] { "Health" }
                }
            }
        };

        var spec = new
        {
            openapi = "3.0.0",
            info = new
            {
                title = "IoT Robot Control & Telemetry API",
                version = "1.0.0",
                description = "RESTful API for controlling IoT robots and monitoring telemetry data",
                contact = new
                {
                    name = "API Support",
                    email = "support@robotapi.example.com"
                }
            },
            servers = new[]
            {
                new { url = "http://localhost:5000", description = "Development server" }
            },
            paths,
            components = new
            {
                securitySchemes = new
                {
                    BearerAuth = new
                    {
                        type = "http",
                        scheme = "bearer",
                        description = "JWT Bearer token authentication. Use mock tokens: admin-token, operator-token, viewer-token"
                    }
                }
            }
        };

        return Results.Ok(spec);
    }
}
