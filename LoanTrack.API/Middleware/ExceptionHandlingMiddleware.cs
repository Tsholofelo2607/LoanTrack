using System.Net;
using System.Text.Json;

namespace LoanTrack.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Let the request continue down the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full exception internally — this is what you check, not what the client sees
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // RFC 7807 Problem Details — a standard error response shape
            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An unexpected error occurred.",
                status = 500,
                detail = "Please contact support if the problem persists.",
                traceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}