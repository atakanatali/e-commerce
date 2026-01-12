namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Represents the components used to build a rate limit key.
/// </summary>
[Flags]
public enum RateLimitKeyType
{
    /// <summary>
    /// Uses the client IP address.
    /// </summary>
    Ip = 1,

    /// <summary>
    /// Uses the authenticated user identifier.
    /// </summary>
    UserId = 2,

    /// <summary>
    /// Uses the request path.
    /// </summary>
    Path = 4
}
