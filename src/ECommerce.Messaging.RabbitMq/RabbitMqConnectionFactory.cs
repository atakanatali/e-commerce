using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Creates RabbitMQ connections using configured options.
/// </summary>
public sealed class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly RabbitMqOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConnectionFactory"/> class.
    /// </summary>
    /// <param name="options">The RabbitMQ options.</param>
    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates connection
    /// </summary>
    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.User,
            Password = _options.Pass,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection(_options.ServiceName);
    }
}
