using Client.Models;
using Client.Models.OrderModel;

namespace Client.Repositories.Interfaces.Order
{
    public interface IPaymentService
    {
        Task<ResponseModel> MoMoPayment(MoMoRequestModel model);
        Task<ResponseModel> VietQRPayment();
    }
}
