using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StatisticalMicroservice.Model.Message;

namespace StatisticalMicroservice.Repostories
{
    public interface IMessage
    {
        public event Action<OrderGroupByUserData> OnOrderResponseReceived;
        public event Action<ProductGroupByUserData> OnProductResponseReceived;
        public event Action<TotalGroupByUserResponse> OnFinalResponseReceived;
        public void ReceiveMessageProduct();
        public void ReceiveMessageUser();

        public void ReceiveMessageOrder();

        public void SendingMessageStastisticalGroupByUser<T>(T message);

        public void ReceiveMessageStastisticalGroupByUserToProduct();

        public void ReceiveMessageStastisticalGroupByUserToOrder();
    }
}
