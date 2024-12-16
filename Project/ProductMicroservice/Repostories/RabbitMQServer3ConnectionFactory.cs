using RabbitMQ.Client;

namespace ProductMicroservice.Repostories
{
    public class RabbitMQServer3ConnectionFactory
    {
        public ConnectionFactory Factory { get; }
        public RabbitMQServer3ConnectionFactory(IConfiguration _config)
        {
            Factory = new ConnectionFactory
            {
                UserName = _config["34"],
                Password = _config["35"],
                VirtualHost = _config["34"],
                Port = 5672,
                HostName = _config["36"]
            };
        }
    }
}
