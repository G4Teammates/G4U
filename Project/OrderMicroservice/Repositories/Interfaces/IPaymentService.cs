using OrderMicroservice.Models;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Models.PaymentModel.MoMo;
using OrderMicroservice.Models.PaymentModel.PayOsModel;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponseModel> MoMoPayment(MoMoRequestFromClient requestClient);
        Task<ResponseModel> VierQRPayment(VietQRRequest request);
        Task<ResponseModel> IpnMoMo(MoMoIPNResquest request);
        Task<ResponseModel> Paid(PaidModel model);
    }
}
