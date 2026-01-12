using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.RateLimiting;

/// <summary>
/// Provides rate limiting registration helpers for dependency injection.
/// </summary>
public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis-backed rate limiting services using configuration settings.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRedisRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        return services;
    }
}
