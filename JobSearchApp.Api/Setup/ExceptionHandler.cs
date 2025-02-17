using System.Text.Json;
using JobSearchApp.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using ILogger = Serilog.ILogger;

namespace JobSearchApp.Api.Setup;

public class ExceptionHandler(ILogger logger) : IExceptionHandler
{
    private readonly ILogger _logger = logger.ForContext<ExceptionHandler>();

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.Error(exception, "An error occurred");
        var response = httpContext.Response;
        response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new { error = exception.Message });
        response.StatusCode = exception switch
        {
            ExceptionWithStatusCode e => (int)e.StatusCode,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        await response.WriteAsync(result, cancellationToken);
        return true;
    }
}