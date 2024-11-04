namespace ProductMicroservice.Repostories
{
    public interface IMessage
    {
        public void SendingMessage<T>(T message);
        public void ReceiveMessage();
    }
}
