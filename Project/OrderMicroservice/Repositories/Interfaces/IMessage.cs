namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void SendingMessageStatistiscal<T>(T message);

        public void ReceiveMessageCheckPurchased();

        //StastisticalGroupByUserToProduct
        public void SendingMessageStastisticalGroupByUserToOrder<T>(T message);
        public void ReceiveMessageStastisticalGroupByUserToOrder();
    }
}
