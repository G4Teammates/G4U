using CommentMicroservice.Models.Message;

namespace CommentMicroservice.Repositories
{
    public interface IMessage
    {
        //check user da mua game
        public event Action<CheckPurchasedResponse> OnResponseReceived;
        public void SendingMessageCheckPurchased<T>(T message);
        public void ReceiveMessageCheckPurchased();
        public void ReceiveMessageFromUser();
    }
}
