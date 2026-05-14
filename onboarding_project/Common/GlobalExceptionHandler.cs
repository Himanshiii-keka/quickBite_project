using Microsoft.AspNetCore.Diagnostics;
using startup_project.Models;

namespace startup_project.Common
{
    /// <summary>
    /// Catches any exception that escapes the service/controller layer and returns a
    /// structured JSON 500 response instead of the raw ASP.NET exception page.
    /// Registered via builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()
    /// and activated by app.UseExceptionHandler() in Program.cs.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorMessageResponse { Message = "An unexpected error occurred. Please try again later." },
                cancellationToken);

            return true;
        }
    }
}
