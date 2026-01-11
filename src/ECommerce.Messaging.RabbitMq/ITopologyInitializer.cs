namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Initializes messaging topology for a service.
/// </summary>
public interface ITopologyInitializer
{
    /// <summary>
    /// Initializes queues, exchanges, and bindings.
    /// </summary>
    void Initialize();
}
