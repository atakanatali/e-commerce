namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Represents configuration settings for Redis-backed rate limiting.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of requests allowed in the window.
    /// </summary>
    public int Requests { get; set; } = 100;

    /// <summary>
    /// Gets or sets the window size in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the key prefix used when generating rate limit keys.
    /// </summary>
    public string KeyPrefix { get; set; } = "rate-limit";
}
