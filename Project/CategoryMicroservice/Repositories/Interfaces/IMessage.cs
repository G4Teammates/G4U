using CategoryMicroservice.Models.DTO;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public event Action<CategoryDeleteResponse> OnCategoryResponseReceived;
        //delete-cate
        public void SendingMessage<T>(T message);
        public void ReceiveMessage();
        //checl-exist-cate
        public void ReceiveMessageCheckExist();
        public void SendingMessageCheckExist<T>(T message);

    }
}
