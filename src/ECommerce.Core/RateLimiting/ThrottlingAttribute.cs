using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Provides attribute-based Redis rate limiting for controller actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ThrottlingAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingAttribute"/> class.
    /// </summary>
    /// <param name="key">The key components used to build the rate limit key.</param>
    /// <param name="duration">The size of the rate limit window.</param>
    /// <param name="type">The time unit for the window.</param>
    /// <param name="limit">The number of requests allowed within the window.</param>
    /// <param name="limitType">The time unit for the limit window.</param>
    public ThrottlingAttribute(
        RateLimitKeyType key = RateLimitKeyType.Ip,
        int duration = 1,
        RateLimitTimeUnit type = RateLimitTimeUnit.Minute,
        int limit = 100,
        RateLimitTimeUnit limitType = RateLimitTimeUnit.Minute)
    {
        Key = key;
        Duration = duration;
        Type = type;
        Limit = limit;
        LimitType = limitType;
    }

    /// <summary>
    /// Gets the key components used to build the rate limit key.
    /// </summary>
    public RateLimitKeyType Key { get; }

    /// <summary>
    /// Gets the size of the rate limit window.
    /// </summary>
    public int Duration { get; }

    /// <summary>
    /// Gets the time unit for the rate limit window.
    /// </summary>
    public RateLimitTimeUnit Type { get; }

    /// <summary>
    /// Gets the number of requests allowed within the window.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Gets the time unit for the limit window.
    /// </summary>
    public RateLimitTimeUnit LimitType { get; }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var limiter = services.GetRequiredService<IRateLimiter>();
        var options = services.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

        if (!options.Enabled)
        {
            await next();
            return;
        }

        var key = BuildKey(context, options.KeyPrefix);
        var effectiveLimit = Limit > 0 ? Limit : options.Requests;
        var windowSeconds = Duration > 0
            ? Duration * (int)Type
            : options.WindowSeconds;
        if (LimitType != Type && Duration > 0)
        {
            windowSeconds = Duration * (int)LimitType;
        }

        var result = await limiter.CheckAsync(
            key,
            effectiveLimit,
            TimeSpan.FromSeconds(windowSeconds),
            context.HttpContext.RequestAborted);

        context.HttpContext.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Reset"] = result.ResetAt.ToUnixTimeSeconds().ToString();

        if (!result.IsAllowed)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var retryAfterSeconds = Math.Max((int)Math.Ceiling(result.RetryAfter.TotalSeconds), 0);
            context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds;
            return;
        }

        await next();
    }

    private string BuildKey(ActionExecutingContext context, string prefix)
    {
        var parts = new List<string> { prefix };

        if (Key.HasFlag(RateLimitKeyType.Ip))
        {
            var forwardedFor = context.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            var ip = !string.IsNullOrWhiteSpace(forwardedFor)
                ? forwardedFor.Split(',')[0].Trim()
                : context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            parts.Add(ip);
        }

        if (Key.HasFlag(RateLimitKeyType.UserId))
        {
            var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.HttpContext.User?.FindFirstValue("sub")
                ?? "anonymous";
            parts.Add(userId);
        }

        if (Key.HasFlag(RateLimitKeyType.Path))
        {
            parts.Add(context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "root");
        }

        return string.Join(':', parts);
    }
}
