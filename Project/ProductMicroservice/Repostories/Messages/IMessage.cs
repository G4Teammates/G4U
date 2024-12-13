using ProductMicroservice.Models.Message;

namespace ProductMicroservice.Repostories.Messages
{
    public interface IMessage
    {
        public event Action<CategoryCheckExistResponse> OnCategoryResponseReceived;
        public event Action<UserCheckExistResponse> OnUserResponseReceived;
        public event Action<OrderItemsResponse> OnOrderItemsResponseReceived;
        // stastistical
        /*public void SendingMessageStatistiscal<T>(T message);*/
        //delete-cate
        /*public void SendingMessage<T>(T message);*/
        public void ReceiveMessage();
        //check-exist-cate
       /* public void SendingMessageCheckExistCategory<T>(T message);*/
        public void ReceiveMessageCheckExistCategory();
        //check-exist-user
        /*public void SendingMessageCheckExistUserName<T>(T message);*/
        public void ReceiveMessageCheckExistUserName();
        //StastisticalGroupByUserToProduct
        /*public void SendingMessageStastisticalGroupByUserToProduct<T>(T message);*/
        public void ReceiveMessageStastisticalGroupByUserToProduct();
        //receive-order-to-product
        public void ReceiveMessageSoldProduct();
        //updateusername
        public void ReceiveMessageFromUser();
        //sending message
        public void SendingMessage<T>(T message, string exchangeName, string queueName, string routingKey, string exchangeType, bool exchangeDurable, bool queueDurable, bool exclusive, bool autoDelete);
    }
}
