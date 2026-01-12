namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Represents a rate limiter capable of checking request limits.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Checks whether the given key is allowed to proceed.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="limit">The maximum number of requests allowed within the window.</param>
    /// <param name="window">The time window for the limit.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The rate limit result.</returns>
    Task<RateLimitResult> CheckAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}
