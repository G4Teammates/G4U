using OrderMicroservice.DBContexts.Enum;
using OrderMicroservice.Models;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetOrder(string id);
        Task<ResponseModel> Create(OrderModel order);
        Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel status);
        Task<ResponseModel> GetOrderItems(string id);
    }
}
