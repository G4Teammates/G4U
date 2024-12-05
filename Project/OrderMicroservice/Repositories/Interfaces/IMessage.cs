namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void SendingMessageStatistiscal<T>(T message);

        public void ReceiveMessageCheckPurchased();
        public void SendingMessageCheckPurchase<T>(T message);
        //StastisticalGroupByUserToProduct
        public void SendingMessageStastisticalGroupByUserToOrder<T>(T message);
        public void ReceiveMessageStastisticalGroupByUserToOrder();
        public void SendingMessageProduct<T>(T message);
        public void ReceiveMessageFromUser();
    }
}
