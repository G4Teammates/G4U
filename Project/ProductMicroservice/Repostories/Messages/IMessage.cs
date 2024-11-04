namespace ProductMicroservice.Repostories.Messages
{
    public interface IMessage
    {
        public void SendingMessage<T>(T message);
        public void ReceiveMessage();
    }
}
