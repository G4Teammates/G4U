using Client.Models;
using Client.Models.OrderModel;

namespace Client.Repositories.Interfaces.Order
{
    public interface IOrderService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetOrder(string id);
        //Task<ResponseModel> CreateOrder(OrderDTO order);
        Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel order);
        Task<ResponseModel> GetOrderItems(string id);
    }
}
