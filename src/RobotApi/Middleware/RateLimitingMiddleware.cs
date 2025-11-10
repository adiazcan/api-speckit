using System.Collections.Concurrent;
using System.Security.Claims;

namespace RobotApi.Middleware;

/// <summary>
/// Middleware that enforces rate limiting per operator (100 requests per minute).
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    // Track request counts per operator: operatorId -> (timestamp, count)
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestCounts = new();
    private const int MaxRequestsPerMinute = 100;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health and documentation endpoints
        if (context.Request.Path.StartsWithSegments("/v1/health") ||
            context.Request.Path.StartsWithSegments("/v1/openapi.json"))
        {
            await _next(context);
            return;
        }

        // Extract operator ID from claims
        var operatorId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? context.User.FindFirst("sub")?.Value
                        ?? "anonymous";

        // Get or create request queue for this operator
        var requestQueue = _requestCounts.GetOrAdd(operatorId, _ => new Queue<DateTime>());

        bool rateLimitExceeded = false;
        int requestCount = 0;

        lock (requestQueue)
        {
            var now = DateTime.UtcNow;
            var cutoffTime = now.Subtract(TimeWindow);

            // Remove expired timestamps (older than 1 minute)
            while (requestQueue.Count > 0 && requestQueue.Peek() < cutoffTime)
            {
                requestQueue.Dequeue();
            }

            // Check if rate limit exceeded
            if (requestQueue.Count >= MaxRequestsPerMinute)
            {
                rateLimitExceeded = true;
                requestCount = requestQueue.Count;
            }
            else
            {
                // Add current request timestamp
                requestQueue.Enqueue(now);
            }
        }

        if (rateLimitExceeded)
        {
            _logger.LogWarning(
                "Rate limit exceeded for operator {OperatorId}. Requests in last minute: {Count}",
                operatorId,
                requestCount);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"Maximum {MaxRequestsPerMinute} requests per minute allowed",
                retryAfter = "60 seconds"
            });
            return;
        }

        await _next(context);
    }
}
