using Client.Models;
using Client.Models.OrderModel;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Order;
using Client.Utility;

namespace Client.Repositories.Services.Order
{
    public class PaymentService(IBaseService baseService) : IPaymentService
    {
        private readonly IBaseService _baseService = baseService;
        public async Task<ResponseModel> MoMoPayment(MoMoRequestModel model)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = model,
                Url = StaticTypeApi.APIGateWay + "/Payment/MoMo"
            });
        }

        public async Task<ResponseModel> VietQRPayment(VietQRRequestModel model)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = model,
                Url = StaticTypeApi.APIGateWay + "/Payment/vietqr"
            });
        }
        public async Task<ResponseModel> Paid(PaidModel model)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = model,
                Url = StaticTypeApi.APIGateWay + "/Payment/paid"
            });
        }
    }
}
