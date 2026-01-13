using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;

namespace ECommerce.Core.Logging;

/// <summary>
/// Enriches log events with HTTP context details and correlation identifiers.
/// </summary>
public sealed class HttpContextLogEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CorrelationOptions _correlationOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContextLogEnricher"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="correlationOptions">The correlation configuration options.</param>
    public HttpContextLogEnricher(
        IHttpContextAccessor httpContextAccessor,
        IOptions<CorrelationOptions> correlationOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _correlationOptions = correlationOptions.Value;
    }

    /// <summary>
    /// Enriches a log event with HTTP metadata when available.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The property factory used to create log properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        var traceId = ResolveTraceId(context);
        var correlationId = ResolveCorrelationId(context, traceId);

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Path", context.Request.Path.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Method", context.Request.Method));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("StatusCode", context.Response.StatusCode));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RemoteIp", context.Connection.RemoteIpAddress?.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", context.TraceIdentifier));
    }

    /// <summary>
    /// Resolves the trace identifier for the current request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The trace identifier.</returns>
    private static string ResolveTraceId(HttpContext context)
    {
        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

    /// <summary>
    /// Resolves the correlation identifier from headers or falls back to the trace identifier.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="fallback">The fallback identifier when no correlation header is present.</param>
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
