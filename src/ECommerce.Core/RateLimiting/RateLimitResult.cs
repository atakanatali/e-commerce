namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Represents the outcome of a rate limit check.
/// </summary>
public sealed class RateLimitResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the request is allowed.
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Gets or sets the configured request limit for the window.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the remaining requests in the current window.
    /// </summary>
    public int Remaining { get; set; }

    /// <summary>
    /// Gets or sets the time at which the window resets.
    /// </summary>
    public DateTimeOffset ResetAt { get; set; }

    /// <summary>
    /// Gets or sets the duration clients should wait before retrying.
    /// </summary>
    public TimeSpan RetryAfter { get; set; }
}
