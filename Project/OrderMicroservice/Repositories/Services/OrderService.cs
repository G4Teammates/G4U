using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using OrderMicroservice.DBContexts;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;
using OrderMicroservice.Models.Message;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Models.UserModel;
using OrderMicroservice.Repositories.Interfaces;

namespace OrderMicroservice.Repositories.Services
{
    public class OrderService(OrderDbContext context, IMapper mapper, IMessage message) : IOrderService
    {
        OrderDbContext _context = context;
        IMapper _mapper = mapper;
        IMessage _message = message;

        public async Task<ResponseModel> Create(OrderModel orderModel)
        {
            ResponseModel response = new();
            try
            {


                Order orderEntity = _mapper.Map<Order>(orderModel);
                _context.Orders.Add(orderEntity);
                await _context.SaveChangesAsync();


                var totalRequest = await TotalRequest();
                _message.SendingMessageStatistiscal(totalRequest.Result);

                response.Result = orderModel;
                response.Message = $"Create order success";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to create order. Error: {ex.Message}";
            }
            return response;
        }


        public async Task<ResponseModel> GetAll()
        {
            ResponseModel response = new();
            try
            {
                var orders = await _context.Orders.ToListAsync();
                response.Result = _mapper.Map<ICollection<OrderModel>>(orders); ;
                response.IsSuccess = true;
                response.Message = "Retrieved all orders successfully.";

                var totalRequest = await TotalRequest();
                _message.SendingMessageStatistiscal(totalRequest.Result);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve orders. Error: {ex.Message}";
            }

            return response;
        }


        public async Task<ResponseModel> GetOrderById(string id)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng theo ID
                var order = await _context.Orders.SingleOrDefaultAsync(i => i.Id == id);

                // Kiểm tra nếu không tìm thấy đơn hàng
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with ID '{id}' not found.";
                    return response;
                }

                // Nếu tìm thấy, map và trả về kết quả
                response.Result = _mapper.Map<OrderModel>(order);
                response.IsSuccess = true;
                response.Message = $"Order with ID '{id}' retrieved successfully.";
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve order with ID '{id}'. Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel> GetOrderByTransaction(string id)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng theo ID
                ICollection<Order>? order = await _context.Orders.Where(i => i.PaymentTransactionId!.Contains(id)).ToListAsync();

                // Kiểm tra nếu không tìm thấy đơn hàng
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with payment transaction ID '{id}' not found.";
                    return response;
                }

                // Nếu tìm thấy, map và trả về kết quả
                response.Result = _mapper.Map<ICollection<OrderModel>>(order);
                response.IsSuccess = true;
                response.Message = $"Order with payment transaction ID '{id}' retrieved successfully.";
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve order with payment transaction ID '{id}'. Error: {ex.Message}";
            }

            return response;
        }


        public async Task<ResponseModel> GetOrderItems(string id)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng theo ID
                var order = await _context.Orders.SingleOrDefaultAsync(i => i.Id == id);

                // Kiểm tra nếu không tìm thấy đơn hàng
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with ID '{id}' not found.";
                    return response;
                }

                // Lấy các mục của đơn hàng
                var orderItems = order.Items;

                // Kiểm tra nếu đơn hàng không có mục nào
                if (orderItems == null || !orderItems.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with ID '{id}' does not contain any items.";
                    return response;
                }

                // Map danh sách orderItems sang mô hình OrderItemModel
                response.Result = _mapper.Map<ICollection<OrderItemModel>>(orderItems);
                response.IsSuccess = true;
                response.Message = $"Order items for order '{id}' retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve items for order '{id}'. Error: {ex.Message}";
            }

            return response;
        }


        public async Task<ResponseModel> UpdateStatus(string id, PaymentStatusModel status)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng
                Order? order = await _context.Orders.SingleOrDefaultAsync(i => i.Id == id);
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with ID '{id}' not found.";
                    return response;
                }

                // Kiểm tra sự thay đổi trạng thái
                bool isPaymentStatusChanged = order.PaymentStatus != status.PaymentStatus;
                bool isOrderStatusChanged = order.OrderStatus != status.OrderStatus;

                // Nếu có thay đổi trạng thái
                if (isPaymentStatusChanged || isOrderStatusChanged)
                {
                    // Cập nhật trạng thái nếu thay đổi
                    if (isPaymentStatusChanged)
                    {
                        order.PaymentStatus = status.PaymentStatus;
                    }

                    if (isOrderStatusChanged)
                    {
                        order.OrderStatus = status.OrderStatus;
                    }

                    // Cập nhật thời gian sửa đổi
                    order.UpdatedAt = DateTime.UtcNow;

                    // Cập nhật dữ liệu trong database
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    // Thêm thông báo dựa trên các thay đổi
                    List<string> updateMessages = new();
                    if (isPaymentStatusChanged) updateMessages.Add("Payment status updated successfully");
                    if (isOrderStatusChanged) updateMessages.Add("Order status updated successfully");

                    // Ghép các thông báo lại
                    response.Message = string.Join(" and ", updateMessages);
                    response.Result = _mapper.Map<OrderModel>(order);
                    response.IsSuccess = true;
                }
                else
                {
                    // Nếu không có sự thay đổi nào
                    response.IsSuccess = false;
                    response.Message = "No changes made as both Payment status and Order status are unchanged.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to update order with ID '{id}'. Error: {ex.Message}";
            }

            return response;
        }



        public async Task<ResponseModel> TotalRequest()
        {
            ResponseModel response = new();
            try
            {
                var Pros = await _context.Orders.ToListAsync();
                if (Pros != null)
                {
                    var totalRevenue = Pros.Sum(pro => pro.TotalPrice);
                    var totalRequest = new TotalRequest()
                    {
                        totalRevenue = totalRevenue,
                        updateAt = DateTime.Now,
                    };
                    response.Result = totalRequest;
                    return response;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateTransId(string orderId, string transId)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng
                Order? order = await _context.Orders.SingleOrDefaultAsync(i => i.Id == orderId);
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with ID '{orderId}' not found.";
                    return response;
                }
                if (order.PaymentTransactionId == null)
                {
                    order.PaymentTransactionId = transId;
                    // Cập nhật thời gian sửa đổi
                    order.UpdatedAt = DateTime.UtcNow;

                    // Cập nhật dữ liệu trong database
                    _context.Attach(order);
                    _context.Entry(order).Property(u => u.PaymentTransactionId).IsModified = true; // Chỉ cập nhật trường cần thiết
                    await _context.SaveChangesAsync();

                    response.Result = _mapper.Map<OrderModel>(order);
                    response.IsSuccess = true;
                }
                else
                {
                    // Nếu không có sự thay đổi nào
                    response.IsSuccess = false;
                    response.Message = "No changes made as both Payment status and Order status are unchanged.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to update order with ID '{orderId}'. Error: {ex.Message}";
            }

            return response;

        }

        public async Task<ResponseModel> GetItemsByCustomerId(string id)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng theo ID
                var order = await _context.Orders.SingleOrDefaultAsync(i => i.CustomerId == id);

                // Kiểm tra nếu không tìm thấy đơn hàng
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with CustomerId '{id}' not found.";
                    return response;
                }

                // Lấy các mục của đơn hàng
                var orderItems = order.Items;

                // Kiểm tra nếu đơn hàng không có mục nào
                if (orderItems == null || !orderItems.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"Order with CustomerId '{id}' does not contain any items.";
                    return response;
                }

                // Map danh sách orderItems sang mô hình OrderItemModel
                response.Result = _mapper.Map<ICollection<OrderItemModel>>(orderItems);
                response.IsSuccess = true;
                response.Message = $"Order items for CustomerId '{id}' retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve items for CustomerId '{id}'. Error: {ex.Message}";
            }

            return response;
        }
    }
}
