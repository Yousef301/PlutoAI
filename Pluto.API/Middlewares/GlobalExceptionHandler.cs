using System.Security.Authentication;
using FluentValidation;

namespace Pluto.API.Middlewares;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public GlobalExceptionHandler(RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex,
                "An unhandled exception occurred. Trace ID: {TraceId}, Request Path: {RequestPath}, Method: {Method}",
                traceId, context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context,
        Exception exception,
        string traceId)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            InvalidCredentialException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception is ValidationException validationException
                ? string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
                : exception.Message,
            TraceId = traceId
        };

        context.Response.Headers.Add("X-Trace-ID", traceId);

        return context.Response.WriteAsJsonAsync(response);
    }
}