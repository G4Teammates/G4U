using CommentMicroservice.Models.Message;

namespace CommentMicroservice.Repositories
{
    public interface IMessage
    {
        //check user da mua game
        public event Action<CheckPurchasedResponse> OnResponseReceived;
        /*public void SendingMessageCheckPurchased<T>(T message);*/
        public void ReceiveMessageCheckPurchased();
        public void ReceiveMessageFromUser();
        public void SendingMessage<T>(
                                      T message,
                                      string exchangeName,
                                      string queueName,
                                      string routingKey,
                                      string exchangeType,
                                      bool exchangeDurable,
                                      bool queueDurable,
                                      bool exclusive,
                                      bool autoDelete);
    }
}
