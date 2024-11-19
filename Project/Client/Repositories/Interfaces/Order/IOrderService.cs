using Client.Models;
using Client.Models.OrderModel;

namespace Client.Repositories.Interfaces.Order
{
    public interface IOrderService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetOrderById(string id);
        Task<ResponseModel> GetOrderByTransaction(string id);
        Task<ResponseModel> CreateOrder(CreateOrderModel order);
        Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel order);
        Task<ResponseModel> GetOrderItems(string id);
        Task<ResponseModel> GetItemsByCustomerId(string id);
        
    }
}
