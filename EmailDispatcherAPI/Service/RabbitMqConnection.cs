using EmailDispatcherAPI.Contract;
using RabbitMQ.Client;

namespace EmailDispatcherAPI.Service
{
    public sealed class RabbitMqConnection : IRabbitMqConnection
    {
        public IConnection Connection { get; private set; } = default!;

        internal void SetConnection(IConnection connection)
        {
            Connection = connection;
        }
    }

}