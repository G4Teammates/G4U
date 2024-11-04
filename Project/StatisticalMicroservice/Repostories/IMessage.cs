namespace StatisticalMicroservice.Repostories
{
    public interface IMessage
    {
        public void ReceiveMessageProduct();
        public void ReceiveMessageUser();

        public void ReceiveMessageOrder();
    }
}
