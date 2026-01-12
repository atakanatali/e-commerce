using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ECommerce.Core.Redis;

/// <summary>
/// Provides Redis registration helpers for dependency injection.
/// </summary>
public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis connectivity services based on configuration settings.
    /// </summary>
    /// <param name="services">The service collection to register Redis services into.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Redis is enabled but configuration is incomplete.</exception>
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Redis");
        services.Configure<RedisOptions>(section);

        var options = new RedisOptions();
        section.Bind(options);

        if (!options.Enabled)
        {
            return services;
        }

        var configurationOptions = BuildConfigurationOptions(options);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configurationOptions));

        return services;
    }

    private static ConfigurationOptions BuildConfigurationOptions(RedisOptions options)
    {
        ConfigurationOptions configurationOptions;

        if (options.Mode == RedisMode.Single)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("Redis is enabled but no connection string was configured.");
            }

            configurationOptions = ConfigurationOptions.Parse(options.ConnectionString, ignoreUnknown: true);
        }
        else
        {
            if (options.Endpoints.Length == 0)
            {
                throw new InvalidOperationException("Redis is enabled in cluster mode but no endpoints were configured.");
            }

            configurationOptions = new ConfigurationOptions();
            foreach (var endpoint in options.Endpoints)
            {
                configurationOptions.EndPoints.Add(endpoint);
            }
        }

        configurationOptions.Password = string.IsNullOrWhiteSpace(options.Password)
            ? configurationOptions.Password
            : options.Password;
        configurationOptions.Ssl = options.Ssl || configurationOptions.Ssl;
        configurationOptions.DefaultDatabase = options.DefaultDatabase ?? configurationOptions.DefaultDatabase;
        configurationOptions.ClientName = string.IsNullOrWhiteSpace(options.InstanceName)
            ? configurationOptions.ClientName
            : options.InstanceName;
        configurationOptions.AbortOnConnectFail = false;

        return configurationOptions;
    }
}
