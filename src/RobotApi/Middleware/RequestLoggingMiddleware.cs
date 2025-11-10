using System.Diagnostics;
using Serilog.Context;

namespace RobotApi.Middleware;

/// <summary>
/// Middleware that logs HTTP requests and responses with timing information.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID if not present
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();
        
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Use Serilog's LogContext to enrich all logs in this request scope
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} started",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString);

                await _next(context);
                
                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "HTTP {Method} {Path}{QueryString} failed after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}
