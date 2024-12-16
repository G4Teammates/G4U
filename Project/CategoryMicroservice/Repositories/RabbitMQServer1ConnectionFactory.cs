using RabbitMQ.Client;

namespace CategoryMicroservice.Repositories
{
    public class RabbitMQServer1ConnectionFactory
    {
        public ConnectionFactory Factory { get; }

        public RabbitMQServer1ConnectionFactory(IConfiguration _config)
        {
            Factory = new ConnectionFactory
            {
                UserName = _config["25"],
                Password = _config["26"],
                VirtualHost = _config["25"],
                Port = 5672,
                HostName = _config["27"]
            };
        }
    }
}
