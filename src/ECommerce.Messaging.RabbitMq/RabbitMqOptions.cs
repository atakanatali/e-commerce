namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Represents RabbitMQ connection settings.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string Host { get; set; } = "rabbitmq";

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string User { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Pass { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string ServiceName { get; set; } = "order-service";
}
