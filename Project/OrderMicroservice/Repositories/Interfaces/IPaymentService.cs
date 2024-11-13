using OrderMicroservice.Models;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Models.PaymentModel.MoMo;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponseModel> MoMoPayment(string id, long amount);
        Task<ResponseModel> VierQRPayment(string id, int amount, ICollection<OrderItemModel> items);
        Task<ResponseModel> IpnMoMo(MoMoIPNResquest request);
        Task<ResponseModel> Paid(PaidModel model);
    }
}
