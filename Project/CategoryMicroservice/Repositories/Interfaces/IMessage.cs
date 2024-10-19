using CategoryMicroservice.Models.DTO;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void SendingMessage<T>(T message);

        public void ReceiveMessage();
        public event Action<CategoryDeleteResponse> OnCategoryResponseReceived;
    }
}
