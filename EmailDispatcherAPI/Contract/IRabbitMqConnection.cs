using RabbitMQ.Client;

namespace EmailDispatcherAPI.Contract
{
    public interface IRabbitMqConnection
    {
        IConnection Connection { get; }
    }

}
