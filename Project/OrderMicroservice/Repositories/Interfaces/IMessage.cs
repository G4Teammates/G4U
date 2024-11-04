namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {
        public void SendingMessageStatistiscal<T>(T message);
    }
}
