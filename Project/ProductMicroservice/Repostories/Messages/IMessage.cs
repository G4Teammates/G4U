using ProductMicroservice.Models.Message;

namespace ProductMicroservice.Repostories.Messages
{
    public interface IMessage
    {
        public event Action<CategoryCheckExistResponse> OnCategoryResponseReceived;
        public event Action<UserCheckExistResponse> OnUserResponseReceived;
        // stastistical
        public void SendingMessageStatistiscal<T>(T message);
        //delete-cate
        public void SendingMessage<T>(T message);
        public void ReceiveMessage();
        //check-exist-cate
        public void SendingMessageCheckExistCategory<T>(T message);
        public void ReceiveMessageCheckExistCategory();
        //check-exist-user
        public void SendingMessageCheckExistUserName<T>(T message);
        public void ReceiveMessageCheckExistUserName();
        //StastisticalGroupByUserToProduct
        public void SendingMessageStastisticalGroupByUserToProduct<T>(T message);
        public void ReceiveMessageStastisticalGroupByUserToProduct();
    }
}
