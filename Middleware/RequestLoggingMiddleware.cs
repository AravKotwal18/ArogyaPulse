using System.Diagnostics;

namespace ArogyaPulse.Api.Middleware
{
    /// <summary>
    /// Logs every incoming HTTP request with method, path, status code, and duration.
    /// Provides structured observability for API monitoring.
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
            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path;

            try
            {
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation(
                    "[API] {Method} {Path} → {StatusCode} ({Duration}ms)",
                    method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[API] {Method} {Path} → EXCEPTION ({Duration}ms)",
                    method, path, stopwatch.ElapsedMilliseconds);
                throw; // Re-throw for ExceptionHandlingMiddleware to catch
            }
        }
    }
}
