using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Implements Redis-backed rate limiting using Redis increment and expiry operations.
/// </summary>
public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RateLimitingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisRateLimiter"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    /// <param name="options">The rate limiting options.</param>
    public RedisRateLimiter(IConnectionMultiplexer connectionMultiplexer, IOptions<RateLimitingOptions> options)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<RateLimitResult> CheckAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new RateLimitResult
            {
                IsAllowed = true,
                Limit = limit,
                Remaining = limit,
                ResetAt = DateTimeOffset.UtcNow,
                RetryAfter = TimeSpan.Zero
            };
        }

        if (limit <= 0 || window <= TimeSpan.Zero)
        {
            return new RateLimitResult
            {
                IsAllowed = true,
                Limit = limit,
                Remaining = limit,
                ResetAt = DateTimeOffset.UtcNow,
                RetryAfter = TimeSpan.Zero
            };
        }

        var db = _connectionMultiplexer.GetDatabase();
        var now = DateTimeOffset.UtcNow;

        var current = await db.StringIncrementAsync(key).ConfigureAwait(false);
        if (current == 1)
        {
            await db.KeyExpireAsync(key, window).ConfigureAwait(false);
        }

        var ttl = await db.KeyTimeToLiveAsync(key).ConfigureAwait(false);
        var ttlMs = ttl?.TotalMilliseconds ?? window.TotalMilliseconds;
        var remaining = Math.Max(limit - (int)current, 0);
        var isAllowed = current <= limit;

        var retryAfter = isAllowed
            ? TimeSpan.Zero
            : TimeSpan.FromMilliseconds(Math.Max(ttlMs, 0));

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            Limit = limit,
            Remaining = remaining,
            ResetAt = now.AddMilliseconds(ttlMs),
            RetryAfter = retryAfter
        };
    }
}
