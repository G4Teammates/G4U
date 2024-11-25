using OrderMicroservice.DBContexts.Enum;
using OrderMicroservice.Models;

using OrderMicroservice.Models.Message;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel> GetAll(int pageNumber, int pageSize);
        Task<ResponseModel> GetOrderById(string id, int pageNumber, int pageSize);
        Task<ResponseModel> GetOrderByTransaction(string id, int pageNumber, int pageSize);
        Task<ResponseModel> Create(OrderModel order);
        Task<ResponseModel> UpdateTransId(string orderId, string transId);
        Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel status);
        Task<ResponseModel> GetOrderItems(string id);
        Task<ResponseModel> GetItemsByCustomerId(string id);
        Task<ResponseModel> TotalRequest();
        Task<bool> CheckPurchaseAsync(CheckPurchaseReceive order);
        Task<OrderGroupByUserData> Data(TotalGroupByUserResponse Response);

    }
}
