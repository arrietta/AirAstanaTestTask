using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Presentation.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var user = context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? context.User?.Identity?.Name
                       ?? "anonymous";
            var time = DateTimeOffset.UtcNow;

            _logger.LogError(ex,
                "Exception for user {User} at {Time}: {Message}",
                user,
                time,
                ex.Message);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var body = JsonSerializer.Serialize(new
            {
                error = ex.Message,
                user,
                time
            });
            await context.Response.WriteAsync(body);
        }
    }
}