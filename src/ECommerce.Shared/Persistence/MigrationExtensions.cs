using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Shared.Persistence;

/// <summary>
/// Provides helper methods for applying EF Core migrations with retries.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies EF Core migrations with retry and exponential backoff.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="serviceProvider">The root service provider.</param>
    /// <param name="logger">The logger to use for warnings.</param>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ApplyMigrationsWithRetryAsync<TContext>(
        IServiceProvider serviceProvider,
        ILogger logger,
        int maxRetries = 10)
        where TContext : DbContext
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                await dbContext.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(
                    ex,
                    "Failed to apply migrations for {Context}. Retrying in {DelaySeconds}s (attempt {Attempt}/{MaxRetries}).",
                    typeof(TContext).Name,
                    delay.TotalSeconds,
                    attempt,
                    maxRetries);
                await Task.Delay(delay);
            }
        }
    }
}
