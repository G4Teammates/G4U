using OrderMicroservice.Models.UserModel;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {

        public event Action<FindUsernameModel> OnFindUserModelResponseReceived;
        public void ReceiveMessageExport();


      
      
        /*public void SendingMessageStatistiscal<T>(T message);*/

        public void ReceiveMessageCheckPurchased();
        /*public void SendingMessageCheckPurchase<T>(T message);*/
        //StastisticalGroupByUserToProduct
        /*public void SendingMessageStastisticalGroupByUserToOrder<T>(T message);*/
        public void ReceiveMessageStastisticalGroupByUserToOrder();
        /*public void SendingMessageProduct<T>(T message);*/
        public void ReceiveMessageFromUser();

        //sending message
        public void SendingMessage<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
        public void SendingMessage2<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
    }
}
