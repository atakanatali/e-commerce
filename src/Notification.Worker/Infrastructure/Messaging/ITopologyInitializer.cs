namespace Notification.Worker.Infrastructure.Messaging;

/// <summary>
/// Defines the contract for initializing RabbitMQ topology.
/// </summary>
public interface ITopologyInitializer
{
    /// <summary>
    /// Declares exchanges, queues, and bindings.
    /// </summary>
    void Initialize();
}
