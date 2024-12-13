namespace UserMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void ReceiveMessageExport();

        //checl-exist-user
        public void ReceiveMessageCheckExist();

        //sending message
        public void SendingMessage<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
    }
}
