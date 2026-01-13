using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Core.Logging;

/// <summary>
/// Logs HTTP requests that result in errors or unhandled exceptions.
/// </summary>
public sealed class HttpErrorLoggingMiddleware
{
    private const int InternalServerErrorStatusCode = StatusCodes.Status500InternalServerError;
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpErrorLoggingMiddleware> _logger;
    private readonly CorrelationOptions _correlationOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpErrorLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationOptions">The correlation configuration options.</param>
    public HttpErrorLoggingMiddleware(
        RequestDelegate next,
        ILogger<HttpErrorLoggingMiddleware> logger,
        IOptions<CorrelationOptions> correlationOptions)
    {
        _next = next;
        _logger = logger;
        _correlationOptions = correlationOptions.Value;
    }

    /// <summary>
    /// Invokes the middleware and logs any failures encountered.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                LogHttpError(context, null, context.Response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = InternalServerErrorStatusCode;
            }

            LogHttpError(context, ex, InternalServerErrorStatusCode);
        }
    }

    /// <summary>
    /// Writes a structured error log entry with HTTP metadata.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The exception that occurred, if any.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    private void LogHttpError(HttpContext context, Exception? exception, int statusCode)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var correlationId = ResolveCorrelationId(context, traceId);

        _logger.LogError(
            exception,
            "HTTP {Method} {Path} responded {StatusCode} with TraceId {TraceId} and CorrelationId {CorrelationId}.",
            context.Request.Method,
            context.Request.Path.Value,
            statusCode,
            traceId,
            correlationId);
    }

    /// <summary>
    /// Resolves the correlation identifier from request headers.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="fallback">The fallback identifier if no header exists.</param>
    /// <returns>The resolved correlation identifier.</returns>
    private string ResolveCorrelationId(HttpContext context, string fallback)
    {
        if (context.Request.Headers.TryGetValue(_correlationOptions.HeaderName, out var headerValues)
            && !string.IsNullOrWhiteSpace(headerValues.ToString()))
        {
            return headerValues.ToString();
        }

        return fallback;
    }
}
