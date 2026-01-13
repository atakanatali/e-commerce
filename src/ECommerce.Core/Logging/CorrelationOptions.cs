namespace ECommerce.Core.Logging;

/// <summary>
/// Defines configuration for correlation identifiers.
/// </summary>
public sealed class CorrelationOptions
{
    /// <summary>
    /// Gets or sets the header name that carries the correlation identifier.
    /// </summary>
    public string HeaderName { get; set; } = "X-Correlation-Id";
}
