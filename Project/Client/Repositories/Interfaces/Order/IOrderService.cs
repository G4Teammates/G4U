﻿using Client.Models;
using Client.Models.OrderModel;

namespace Client.Repositories.Interfaces.Order
{
    public interface IOrderService
    {
        Task<ResponseModel> GetAll(int? pagerNumber, int pageSize);
        Task<ResponseModel> GetOrderById(string id, int? pagerNumber, int pageSize);
        Task<ResponseModel> GetOrderByTransaction(string id, int? pagerNumber, int pageSize);
        Task<ResponseModel> CreateOrder(CreateOrderModel order);
        Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel order);
        Task<ResponseModel> GetOrderItems(string id);
        Task<ResponseModel> GetItemsByCustomerId(string id);
        
    }
}
