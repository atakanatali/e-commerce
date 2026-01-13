using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Creates RabbitMQ connections using configured options.
/// </summary>
public sealed class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConnectionFactory"/> class.
    /// </summary>
    /// <param name="options">The RabbitMQ options.</param>
    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
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

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                return factory.CreateConnection(_options.ServiceName);
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ connection attempt {Attempt}/{MaxAttempts} to {Host} failed. Retrying in {DelaySeconds}s.",
                    attempt,
                    MaxAttempts,
                    _options.Host,
                    RetryDelay.TotalSeconds);
                Thread.Sleep(RetryDelay);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RabbitMQ connection failed after {MaxAttempts} attempts to {Host}.",
                    MaxAttempts,
                    _options.Host);
                throw;
            }
        }

        throw new InvalidOperationException("RabbitMQ connection attempts exhausted.");
    }
}
