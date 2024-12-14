using RabbitMQ.Client;

namespace StatisticalMicroservice.Repostories
{
    public class RabbitMQServer2ConnectionFactory
    {
        public ConnectionFactory Factory { get; }

        public RabbitMQServer2ConnectionFactory(IConfiguration _config)
        {
            Factory = new ConnectionFactory
            {
                UserName = _config["31"],
                Password = _config["32"],
                VirtualHost = _config["31"],
                Port = 5672,
                HostName = _config["33"]
            };
        }
    }
}
