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
                    return CreateErrorResponse($"Order with ID '{id}' not found.");
                }

                // Kiểm tra và cập nhật trạng thái nếu có thay đổi
                bool isUpdated = false;
                List<string> updateMessages = new();

                if (order.PaymentStatus != status.PaymentStatus)
                {
                    order.PaymentStatus = status.PaymentStatus;
                    updateMessages.Add("Payment status updated successfully.");
                    isUpdated = true;
                }

                if (order.OrderStatus != status.OrderStatus)
                {
                    order.OrderStatus = status.OrderStatus;
                    updateMessages.Add("Order status updated successfully.");
                    isUpdated = true;
                }

                if (order.PaymentName != status.PaymentName)
                {
                    order.PaymentName = status.PaymentName;
                    updateMessages.Add("Payment name updated successfully.");
                    isUpdated = true;
                }

                if (order.PaymentMethod != status.PaymentMethod)
                {
                    order.PaymentMethod = status.PaymentMethod;
                    updateMessages.Add("Payment method updated successfully.");
                    isUpdated = true;
                }

                // Nếu không có thay đổi nào
                if (!isUpdated)
                {
                    return CreateErrorResponse("No changes made as all values are unchanged.");
                }

                // Cập nhật thời gian sửa đổi
                order.UpdatedAt = DateTime.UtcNow;

                // Lưu dữ liệu
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Trả về thông báo cập nhật
                response.Message = string.Join(" and ", updateMessages);
                response.Result = _mapper.Map<OrderModel>(order);
                response.IsSuccess = true;

                // Gửi thông điệp thống kê
                var totalRequest = await TotalRequest();
                _message.SendingMessageStatistiscal(totalRequest.Result);

                return response;
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"Failed to update order with ID '{id}'. Error: {ex.Message}");
            }
        }

        // Tạo phản hồi lỗi
        private ResponseModel CreateErrorResponse(string message)
        {
            return new ResponseModel
            {
                IsSuccess = false,
                Message = message
            };
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
                    response.Message = "Not found any Order";
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
                // Tìm tất cả đơn hàng của khách hàng dựa trên CustomerId
                var orders = await _context.Orders
                    .Where(i => i.CustomerId == id)
                    .ToListAsync();

                // Kiểm tra nếu không tìm thấy đơn hàng nào
                if (orders == null || !orders.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"No orders found for CustomerId '{id}'.";
                    return response;
                }

                // Lấy tất cả các items từ danh sách đơn hàng
                var allOrderItems = orders
                    .SelectMany(o => o.Items) // Lấy tất cả items từ tất cả orders
                    .ToList();

                // Kiểm tra nếu không có item nào
                if (allOrderItems == null || !allOrderItems.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"No items found in orders for CustomerId '{id}'.";
                    return response;
                }

                // Nhóm các items theo ProductId để gộp các sản phẩm trùng nhau
                var groupedItems = allOrderItems
                    .GroupBy(item => item.ProductId)
                    .Select(group => new OrderItemModel
                    {
                        ProductId = group.Key,
                        ProductName = group.First().ProductName, // Lấy tên từ một item trong nhóm
                        PublisherName = group.First().PublisherName, // Lấy tên người đăng từ một item trong nhóm
                        ImageUrl = group.First().ImageUrl, // Lấy URL hình ảnh từ một item
                        Quantity = group.Sum(item => item.Quantity), // Tổng số lượng
                        Price = group.First().Price, // Lấy giá từ một item (giả định giá giống nhau)
                                                     // Các thuộc tính TotalPrice và TotalProfit được tính toán tự động trong OrderItemModel
                    })
                    .ToList();

                // Map kết quả sang Result và thiết lập thông báo trả về
                response.Result = groupedItems;
                response.IsSuccess = true;
                response.Message = $"Order items for CustomerId '{id}' retrieved and consolidated successfully.";
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
