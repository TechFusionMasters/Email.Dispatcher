using RabbitMQ.Client;

namespace EmailWorker.Contract
{
    public interface IRabbitMqConnection
    {
        IConnection Connection { get; }
        void SetConnection(IConnection connection);
    }

}
