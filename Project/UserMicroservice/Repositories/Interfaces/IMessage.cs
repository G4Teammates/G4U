namespace UserMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void ReceiveMessageExport();

        //checl-exist-user
        public void ReceiveMessageCheckExist();

        //sending message
        public void SendingMessage2<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
        public void SendingMessage3<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
    }
}
