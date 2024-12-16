using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using OrderMicroservice.DBContexts;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.DBContexts.Enum;
using OrderMicroservice.Models;
using OrderMicroservice.Models.Message;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Models.UserModel;
using OrderMicroservice.Repositories.Interfaces;
using RabbitMQ.Client;
using System.ComponentModel.Design;
using X.PagedList.Extensions;

namespace OrderMicroservice.Repositories.Services
{
    public class OrderService(OrderDbContext context, IMapper mapper, IMessage message, IHelperService helpService) : IOrderService
    {
        OrderDbContext _context = context;
        IMapper _mapper = mapper;
        IMessage _message = message;
        IHelperService _helperService = helpService;
        public async Task<ResponseModel> Create(OrderModel orderModel)
        {
            ResponseModel response = new();
            try
            {


                Order orderEntity = _mapper.Map<Order>(orderModel);
                _context.Orders.Add(orderEntity);
                await _context.SaveChangesAsync();


                var totalRequest = await TotalRequest();
                _message.SendingMessage2(totalRequest.Result, "Stastistical", "totalOrder_for_stastistical", "totalOrder_for_stastistical", ExchangeType.Direct, true, false, false, false);
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


        public async Task<ResponseModel> GetAll(int pageNumber, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var orders = await _context.Orders.ToListAsync();
                response.Result = _mapper.Map<ICollection<OrderModel>>(orders).ToPagedList(pageNumber, pageSize); ;
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


        public async Task<ResponseModel> GetOrderById(string id, int pageNumber, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng dựa trên từng loại id
                var ordersByOrderId = await _context.Orders.Where(o => o.Id == id).ToListAsync();
                var ordersByCustomerId = await _context.Orders.Where(o => o.CustomerId == id).ToListAsync();
                var ordersByProductId = await _context.Orders
                    .Where(o => o.Items.Any(i => i.ProductId == id))
                    .ToListAsync();


                if (ordersByOrderId.Any())
                {
                    // Nếu tìm thấy theo Order.Id
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByOrderId).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order with ID '{id}' found by Order ID.";
                }
                else if (ordersByCustomerId.Any())
                {
                    // Nếu tìm thấy theo CustomerId
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByCustomerId).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order with Customer ID '{id}' found.";
                }
                else if (ordersByProductId.Any())
                {
                    // Nếu tìm thấy theo ProductId trong Items
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByProductId).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order containing Product ID '{id}' found.";
                }
                else
                {
                    // Nếu không tìm thấy
                    response.IsSuccess = false;
                    response.Message = $"No orders found with ID '{id}'.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve order with ID '{id}'. Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel> GetOrderByTransaction(string id, int pageNumber, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                // Tìm đơn hàng theo Transaction ID
                var ordersByTransactionId = await _context.Orders
                    .Where(i => i.PaymentTransactionId != null && i.PaymentTransactionId.Contains(id))
                    .ToListAsync();

                // Tìm đơn hàng theo Product Name trong Items
                var ordersByProductName = await _context.Orders
                    .Where(o => o.Items.Any(i => i.ProductName.ToLower().Contains(id.ToLower())))
                    .ToListAsync();

                // Tìm đơn hàng theo Publisher Name trong Items
                var ordersByPublisherName = await _context.Orders
                    .Where(o => o.Items.Any(i => i.PublisherName.ToLower().Contains(id.ToLower())))
                    .ToListAsync();

                // Kiểm tra kết quả và thiết lập phản hồi
                if (ordersByProductName.Any())
                {
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByProductName).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order containing Product Name '{id}' found.";
                }
                else if (ordersByTransactionId.Any())
                {
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByTransactionId).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order with payment transaction ID '{id}' retrieved successfully.";
                }
                else if (ordersByPublisherName.Any())
                {
                    response.Result = _mapper.Map<ICollection<OrderModel>>(ordersByPublisherName).ToPagedList(pageNumber, pageSize);
                    response.IsSuccess = true;
                    response.Message = $"Order with payment publisher name '{id}' retrieved successfully.";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"No orders found matching ID '{id}'.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo lỗi
                response.IsSuccess = false;
                response.Message = $"Failed to retrieve order. Error: {ex.Message}";
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

                if (order.PaymentMethod == DBContexts.Enum.PaymentMethod.Free)
                {
                    var updateSoldResponse = UpdateSold(new ProductSoldRequest
                    {
                        IsExist = true,
                        ProductSoldModels = order.Items.Select(item => new ProductSoldModel
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        }).ToList()
                    });
                    if (!updateSoldResponse.IsSuccess)
                    {
                        return new ResponseModel
                        {
                            IsSuccess = false,
                            Message = "Failed to update sold product information"
                        };
                    }
                }


                // Gửi thông điệp thống kê
                var totalRequest = await TotalRequest();
                _message.SendingMessage2(totalRequest.Result, "Stastistical", "totalOrder_for_stastistical", "totalOrder_for_stastistical", ExchangeType.Direct, true, false, false, false);


                // Trả về thông báo cập nhật
                response.Message = string.Join(" and ", updateMessages);
                response.Result = _mapper.Map<OrderModel>(order);
                response.IsSuccess = true;

            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"Failed to update order with ID '{id}'. Error: {ex.Message}");
            }
            return response;
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


        public ResponseModel UpdateSold(ProductSoldRequest request)
        {
            try
            {
                _message.SendingMessage2(request.ProductSoldModels, "Product", "order_for_sold_product", "order_for_sold_product", ExchangeType.Direct, true, false, false, false);
                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Update sold product success"
                };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = $"Failed to update sold product: {ex.Message}"
                };
            }
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
                    .Where(i =>
                        i.CustomerId == id &&
                        i.OrderStatus == OrderStatus.Paid)
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



        public async Task<ResponseModel> GroupByProfitOrder(DateTime dateTime)
        {
            ResponseModel response = new();
            try
            {
                response = await GetAll(1, 9999);
                if (!response.IsSuccess)
                {
                    return response;
                }
                var pagedOrders = response.Result as X.PagedList.IPagedList<OrderModel>;
                if (pagedOrders == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Failed to cast Result to IPagedList<OrderModel>."
                    };
                }

                ICollection<OrderModel> orders = pagedOrders.ToList();


                if (orders == null)
                {
                    return new ResponseModel { IsSuccess = false, Message = "Invalid data format." };
                }
                orders = orders.Where(p =>
                        p.UpdatedAt.Year == dateTime.Year &&
                        p.UpdatedAt.Month == dateTime.Month
                    ).ToList();

                List<ExportUserModel> groupedData = orders
                    .SelectMany(order => order.Items) // Lấy tất cả OrderItemModel từ các đơn hàng
                    .GroupBy(item => item.PublisherName) // Nhóm theo PublisherName
                    .Select(group => new ExportUserModel
                    {
                        PublisherName = group.Key, // Tên của publisher
                        TotalProfit = group.Sum(item => item.TotalProfit), // Tổng lợi nhuận
                        TotalPrice = group.Sum(item => item.TotalPrice) // Tổng giá trị đơn hàng
                    })
                    .ToList();
                ExportResult result = new ExportResult()
                {
                    ExportProfits = groupedData,
                    CreateAt = DateTime.UtcNow
                };
                
                response.Result = result;
                response.IsSuccess = true;
                response.Message = "Group by success";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }





        public async Task<bool> CheckPurchaseAsync(CheckPurchaseReceive order)
        {
            // Check if any order contains the specified UserId
            var userOrder = await Task.Run(() => _context.Orders
                .Where(o => o.CustomerId == order.UserId).ToList());
            // If no order is found for the given UserId, return false
            if (userOrder == null)
            {
                return false;
            }

            // Check if the ProductId exists in the items list of the order
            var iexist = userOrder.Where(x=> x.Items.Any(item => item.ProductId == order.ProductId));
            bool isProductInOrder = false;
            if (iexist.Count() > 0)
            {
                isProductInOrder = true;
                return isProductInOrder;
            }
            else
            {
                return isProductInOrder;
            }

        }
        public async Task<OrderGroupByUserData> Data(TotalGroupByUserResponse response)
        {
            var result = new OrderGroupByUserData();

            /*// Đảm bảo response.CreateAt là UTC và có thời gian là 00:00:00
            var startOfDayUtc = DateTime.SpecifyKind(response.CreateAt.Date, DateTimeKind.Utc);

            var orders = await _context.Orders
                .Where(p => p.Items.Any(x=> x.PublisherName==response.UserName)  && p.CreatedAt <= startOfDayUtc).ToListAsync();*/

            // Lấy tháng và năm từ response.CreateAt
            var targetMonth = response.CreateAt.Month;
            var targetYear = response.CreateAt.Year;

            // Query các sản phẩm có UserName trùng và tháng, năm của CreatedAt trùng với tháng, năm được truyền vào
            var orders = await _context.Orders
                .Where(p => p.Items.Any(x => x.PublisherName == response.UserName) &&
                            p.CreatedAt.Month == targetMonth &&
                            p.CreatedAt.Year == targetYear)
                .ToListAsync();

            result.Revenue = orders.Sum(x => x.TotalProfit);

            return result;
        }
        public async Task<ResponseModel> UpdateUserName(UpdateUserNameModel model)
        {
            ResponseModel response = new();

            try
            {
                // Tìm tất cả đơn hàng có Items chứa PublisherName trùng với oldusername
                var orders = await _context.Orders.Where(o => o.Items.Any(i => i.PublisherName == model.OldUserName)).ToListAsync();

                if (orders.Count > 0)
                {
                    // Cập nhật PublisherName của tất cả các sản phẩm trong danh sách Items
                    foreach (var order in orders)
                    {
                        if (order.Items != null)
                        {
                            foreach (var item in order.Items)
                            {
                                if (item.PublisherName == model.OldUserName)
                                {
                                    item.PublisherName = model.NewUserName;
                                }
                            }
                        }
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    response.Message = "Update successfully";
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "Not found any order with matching PublisherName";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

    }
}
