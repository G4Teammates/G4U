using CategoryMicroservice.Models.DTO;
using RabbitMQ.Client;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public event Action<CategoryDeleteResponse> OnCategoryResponseReceived;
        //delete-cate
        public void ReceiveMessage();
        //checl-exist-cate
        public void ReceiveMessageCheckExist();
        //sending message
        public void SendingMessage<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType , bool exchangeDurable , bool queueDurable, bool exclusive, bool autoDelete);

    }
}
