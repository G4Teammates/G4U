using OrderMicroservice.Models.UserModel;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public event Action<FindUsernameModel> OnFindUserModelResponseReceived;
        public void SendingMessageStatistiscal<T>(T message);

        public void ReceiveMessageCheckPurchased();

        public void SendingMessageExport<T>(T message);
        public void ReceiveMessageExport();

        public void SendingMessageCheckPurchase<T>(T message);
        //StastisticalGroupByUserToProduct
        public void SendingMessageStastisticalGroupByUserToOrder<T>(T message);
        public void ReceiveMessageStastisticalGroupByUserToOrder();
        public void SendingMessageProduct<T>(T message);
    }
}
