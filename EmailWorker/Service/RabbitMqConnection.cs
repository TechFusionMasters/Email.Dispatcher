using EmailWorker.Contract;
using RabbitMQ.Client;

namespace EmailWorker.Service
{
    public sealed class RabbitMqConnection : IRabbitMqConnection
    {
        public IConnection Connection { get; private set; } = default!;

        public void SetConnection(IConnection connection)
        {
            Connection = connection;
        }
    }

}