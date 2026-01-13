namespace ECommerce.Core.Logging;

/// <summary>
/// Defines configuration settings for Elasticsearch log delivery.
/// </summary>
public sealed class ElasticsearchLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Elasticsearch logging is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Elasticsearch endpoint URI.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Gets or sets the index prefix used when writing logs.
    /// </summary>
    public string? IndexPrefix { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level as text.
    /// </summary>
    public string? MinimumLevel { get; set; }

    /// <summary>
    /// Gets or sets the Elasticsearch username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the Elasticsearch password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to auto-register templates.
    /// </summary>
    public bool AutoRegisterTemplate { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of log events to batch per request.
    /// </summary>
    public int BatchPostingLimit { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the sink period in seconds.
    /// </summary>
    public int PeriodSeconds { get; set; } = 2;
}
