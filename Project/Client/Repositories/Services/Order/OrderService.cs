using Client.Models;
using Client.Models.OrderModel;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Order;
using Client.Utility;

namespace Client.Repositories.Services.Order
{
    public class OrderService(IBaseService baseService) : IOrderService
    {
        private readonly IBaseService _baseService = baseService;
        public async Task<ResponseModel> GetAll()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Order"
            });
        }

        public async Task<ResponseModel> GetOrder(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Order/" + id
            });
        }

        public async Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel order)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = order,
                Url = StaticTypeApi.APIGateWay + "/Order" + id
            });
        }

        public async Task<ResponseModel> GetOrderItems(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Order" + id + "/items"
            });
        }
    }
}
