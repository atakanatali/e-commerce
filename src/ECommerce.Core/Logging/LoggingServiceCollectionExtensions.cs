using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace ECommerce.Core.Logging;

/// <summary>
/// Provides dependency injection helpers for configuring Elasticsearch logging.
/// </summary>
public static class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// Registers logging configuration and bootstrap logger for the service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The logical service name for log enrichment.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceName = null)
    {
        services.Configure<ElasticsearchLoggingOptions>(configuration.GetSection("Logging:Elasticsearch"));
        services.Configure<CorrelationOptions>(configuration.GetSection("Correlation"));
        services.AddHttpContextAccessor();

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        Log.Logger = BuildLoggerConfiguration(configuration, environmentName, serviceName, null, includeHttpContextEnricher: false)
            .CreateLogger();

        Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions.AddLogging(
            services,
            builder => builder.ClearProviders());

        return services;
    }

    /// <summary>
    /// Configures Serilog for the host builder and Elasticsearch sink.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration to populate.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environmentName">The environment name used for enrichment.</param>
    /// <param name="services">The service provider for resolving enrichers.</param>
    /// <param name="serviceName">The logical service name.</param>
    public static void ConfigureSerilog(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string environmentName,
        IServiceProvider services,
        string? serviceName = null)
    {
        BuildLoggerConfiguration(configuration, environmentName, serviceName, services, includeHttpContextEnricher: true, loggerConfiguration);
    }

    /// <summary>
    /// Builds the base Serilog configuration for the service.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environmentName">The environment name used for enrichment.</param>
    /// <param name="serviceName">The logical service name.</param>
    /// <param name="services">The service provider for resolving enrichers.</param>
    /// <param name="includeHttpContextEnricher">Whether to include HTTP context enrichment.</param>
    /// <param name="loggerConfiguration">An optional existing configuration to update.</param>
    /// <returns>The populated logger configuration.</returns>
    private static LoggerConfiguration BuildLoggerConfiguration(
        IConfiguration configuration,
        string environmentName,
        string? serviceName,
        IServiceProvider? services,
        bool includeHttpContextEnricher,
        LoggerConfiguration? loggerConfiguration = null)
    {
        var options = new ElasticsearchLoggingOptions();
        configuration.GetSection("Logging:Elasticsearch").Bind(options);

        var resolvedServiceName = ResolveServiceName(serviceName);
        var indexPrefix = string.IsNullOrWhiteSpace(options.IndexPrefix) ? "logs" : options.IndexPrefix;
        var minimumLevel = ParseMinimumLevel(options.MinimumLevel);

        var resolvedLoggerConfiguration = loggerConfiguration ?? new LoggerConfiguration();

        resolvedLoggerConfiguration
            .MinimumLevel.Is(minimumLevel)
            .Enrich.WithProperty("ServiceName", resolvedServiceName)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("ApplicationVersion", ResolveApplicationVersion())
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext();

        if (includeHttpContextEnricher)
        {
            AddHttpContextEnricher(resolvedLoggerConfiguration, services);
        }

        if (options.Enabled && !string.IsNullOrWhiteSpace(options.Uri))
        {
            var sinkOptions = new ElasticsearchSinkOptions(new Uri(options.Uri))
            {
                IndexFormat = $"{indexPrefix}-{NormalizeServiceName(resolvedServiceName)}-{{0:yyyy.MM.dd}}",
                AutoRegisterTemplate = options.AutoRegisterTemplate,
                BatchPostingLimit = options.BatchPostingLimit,
                Period = TimeSpan.FromSeconds(options.PeriodSeconds > 0 ? options.PeriodSeconds : 2)
            };

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                sinkOptions.ModifyConnectionSettings = connectionSettings =>
                    connectionSettings.BasicAuthentication(options.Username, options.Password);
            }

            resolvedLoggerConfiguration.WriteTo.Elasticsearch(sinkOptions);
        }

        return resolvedLoggerConfiguration;
    }

    /// <summary>
    /// Resolves the logical service name for log enrichment.
    /// </summary>
    /// <param name="serviceName">The service name provided by the caller.</param>
    /// <returns>The resolved service name.</returns>
    private static string ResolveServiceName(string? serviceName)
    {
        return string.IsNullOrWhiteSpace(serviceName)
            ? AppDomain.CurrentDomain.FriendlyName
            : serviceName;
    }

    /// <summary>
    /// Resolves the application version for log enrichment.
    /// </summary>
    /// <returns>The resolved application version.</returns>
    private static string ResolveApplicationVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Normalizes a service name for safe index naming.
    /// </summary>
    /// <param name="serviceName">The service name to normalize.</param>
    /// <returns>The normalized service name.</returns>
    private static string NormalizeServiceName(string serviceName)
    {
        return serviceName.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    /// <summary>
    /// Parses a minimum log level from configuration text.
    /// </summary>
    /// <param name="minimumLevel">The configured minimum level.</param>
    /// <returns>The parsed log event level.</returns>
    private static LogEventLevel ParseMinimumLevel(string? minimumLevel)
    {
        return Enum.TryParse(minimumLevel, ignoreCase: true, out LogEventLevel level)
            ? level
            : LogEventLevel.Information;
    }

    /// <summary>
    /// Adds the HTTP context enricher when service registrations are available.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration to update.</param>
    /// <param name="services">The service provider for resolving dependencies.</param>
    private static void AddHttpContextEnricher(LoggerConfiguration loggerConfiguration, IServiceProvider? services)
    {
        if (services is null)
        {
            return;
        }

        var httpContextAccessor = services.GetService<IHttpContextAccessor>();
        var correlationOptions = services.GetService<IOptions<CorrelationOptions>>();

        if (httpContextAccessor is null || correlationOptions is null)
        {
            return;
        }

        loggerConfiguration.Enrich.With(new HttpContextLogEnricher(httpContextAccessor, correlationOptions));
    }

    
}
