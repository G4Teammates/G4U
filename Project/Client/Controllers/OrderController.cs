using AutoMapper;
using Azure;
using Client.Models;
using Client.Models.Enum.OrderEnum;
using Client.Models.OrderModel;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Order;
using Client.Repositories.Interfaces.Product;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace Client.Controllers
{
    public class OrderController(IOrderService orderService, IPaymentService paymentService, IMapper mapper, IHelperService helperService, ITokenProvider tokenProvider, IRepoProduct productService) : Controller
    {
        #region declare
        private readonly IMapper _mapper = mapper;
        private readonly IOrderService _orderService = orderService;
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IHelperService _helperService = helperService;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly IRepoProduct _productService = productService;
        private string JWT = "JWT";
        private string IsLogin = "IsLogin";
        private string RememberMe = "RememberMe";
        private string Cart = "cart";
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> History(int? page, int pageSize = 10)
        {
            try
            {
                // Lấy Id từ token
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                string id = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;

                // Khởi tạo ViewData
                ViewData["HistoryAction"] = nameof(History);
                int pageNumber = (page ?? 1); // Trang hiện tại
                OrderViewModel orders = new()
                {
                    Orders = []

                };

                // Gọi API để lấy tất cả dữ liệu
                ResponseModel? response = await _orderService.GetOrderById(id, 1, 999);
                if (response != null && response.IsSuccess)
                {
                    // Chuyển đổi dữ liệu từ API
                    var allOrders = JsonConvert.DeserializeObject<ICollection<OrderModel>>(Convert.ToString(response.Result!.ToString()!));

                    // Lọc các đơn hàng đã thanh toán
                    var filteredOrders = allOrders!.Where(order => order.OrderStatus == OrderStatus.Paid).ToList();

                    // Áp dụng phân trang với Skip và Take
                    var pagedOrders = filteredOrders
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    // Thiết lập thông tin phân trang
                    orders.Orders = pagedOrders;
                    orders.pageNumber = pageNumber;
                    orders.pageSize = pageSize;
                    orders.totalItem = filteredOrders.Count;
                    orders.pageCount = (int)Math.Ceiling(filteredOrders.Count / (double)pageSize);

                    TempData["success"] = "Load order history is success";
                }
                else
                {
                    if (orders.Orders.Count == 0)
                    {
                        //TempData["success"] = "You are newbie huh? Buy game now!!";

                    }
                    else
                    {
                        TempData["error"] = response?.Message ?? "An unexpected error occurred while fetching the orders.";

                    }
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                TempData["error"] = $"An error occurred: {ex.Message}";
                return View(new OrderViewModel() { Orders = null }); // Trả về View rỗng hoặc một đối tượng ViewModel mặc định
            }
        }



        public async Task<IActionResult> Checkout(string orderJson, PaymentMethod paymentMethod)
        {
            bool isLogin = false;
            bool rememberMe = Convert.ToBoolean(_tokenProvider.GetToken(RememberMe));
            string token;

            if (rememberMe)
            {
                token = _tokenProvider.GetToken(JWT);
                isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin));
            }
            else
            {
                token = _tokenProvider.GetToken(JWT, false);
                isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin, false));
            }

            if (token == null || !isLogin)
            {
                TempData["error"] = "You should login first";
                return RedirectToAction("Cart", "User");
            }


            CartModel cart = JsonConvert.DeserializeObject<CartModel>(orderJson);
            cart.PaymentMethod = paymentMethod;
            cart.Order.PaymentName = "Pending";

            if (cart.Order.Items == null)
            {
                TempData["Error"] = "Cart is empty";
                return RedirectToAction("Cart", "User");
            }

            if (cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.Wallet)
            {
                return await ProcessWalletPayment(cart);
            }
            else if (cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.CreditCard)
            {
                return await ProcessCreditCardPayment(cart);
            }
            else if (cart.Order.TotalPrice == 0)
            {
                return await ProcessFreePayment(cart);
            }

            return RedirectToAction("PaymentFailure");
        }

        #region Process Payment Methods
        private async Task<IActionResult> ProcessWalletPayment(CartModel cart)
        {
            CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
            ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);

            if (responseCreateOrder.IsSuccess)
            {
                OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
                MoMoRequestModel request = new MoMoRequestModel(newOrder.Id, (long)cart.Order.TotalPrice);
                ResponseModel responsePayment = await _paymentService.MoMoPayment(request);

                if (responsePayment.IsSuccess)
                {
                    HttpContext.Response.Cookies.Delete("cart");
                    return Redirect(responsePayment.Result.ToString());
                }
                else
                {
                    return RedirectToAction("PaymentFailure");
                }
            }

            return RedirectToAction("PaymentFailure");
        }
        private async Task<IActionResult> ProcessCreditCardPayment(CartModel cart)
        {
            CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
            ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);

            if (responseCreateOrder.IsSuccess)
            {
                OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
                VietQRRequestModel request = new VietQRRequestModel()
                {
                    Id = newOrder.Id,
                    Amount = (int)cart.Order.TotalPrice,
                    Items = cart.Order.Items
                };
                ResponseModel responsePayment = await _paymentService.VietQRPayment(request);
                HttpContext.Response.Cookies.Append("orderId", newOrder.Id);
                if (responsePayment.IsSuccess)
                {
                    HttpContext.Response.Cookies.Delete("cart");
                    return Redirect(responsePayment.Result.ToString());
                }
                else
                {
                    return RedirectToAction("PaymentFailure");
                }
            }

            return RedirectToAction("PaymentFailure"); // Placeholder, trả về lỗi nếu không xử lý được
        }
        private async Task<IActionResult> ProcessFreePayment(CartModel cart)
        {
            CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
            ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);

            if (responseCreateOrder.IsSuccess)
            {
                OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
                PaymentStatusModel status = new PaymentStatusModel
                {
                    OrderStatus = OrderStatus.Paid,
                    PaymentMethod = PaymentMethod.Free,
                    PaymentName = "Free",
                    PaymentStatus = PaymentStatus.Paid
                };
                ResponseModel responseUpdateOrder = await _orderService.UpdateStatus(newOrder.Id, status);
                if (responseUpdateOrder.IsSuccess)
                {
                    HttpContext.Response.Cookies.Delete("cart");
                    OrderModel freeOrder = JsonConvert.DeserializeObject<OrderModel>(responseUpdateOrder.Result.ToString());

                    return RedirectToAction("PaymentSuccess", "Order", new
                    {
                        orderId = newOrder.Id,
                        amount = 0,
                        resultCode = 0,
                        orderType = "Free",
                        responseTime = ((DateTimeOffset)newOrder.UpdatedAt).ToUnixTimeMilliseconds()
                    });
                }
                else
                {
                    TempData["error"] = responseUpdateOrder.Message;
                    return RedirectToAction("PaymentFailure");
                }
            }
            TempData["error"] = responseCreateOrder.Message;
            return RedirectToAction("PaymentFailure");
        }
        #endregion


        #region Return Payment Status
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string? partnerCode, string? orderId, string? requestId, decimal amount, string? orderInfo, string? orderType, string? transId, int? resultCode, string message, string payType, long responseTime, string extraData, string signature)
        {
            if (resultCode != 0)
            {
                return RedirectToAction(nameof(PaymentFailure));
            }
            var model = new PaymentSuccessModel
            {
                OrderId = orderId,
                Amount = amount,
                OrderType = orderType,
                ResponseTime = responseTime,
            };
            string email = User.FindFirst(ClaimTypes.Email)?.Value;
            if(email == null)
            {
                TempData["error"] = "You should login first";
                return RedirectToAction("Login", "User");
            }

            ResponseModel responseOrder = await _orderService.GetOrderById(orderId,1,1);
            OrderModel order = JsonConvert.DeserializeObject<OrderModel>(responseOrder.Result.ToString());

            if (order == null || order.Items == null)
            {
                TempData["error"] = "Order not found";
                return RedirectToAction("PaymentFailure");
            }

            foreach (var i in order.Items)
            {
                ResponseModel responseIdProduct = await _productService.GetProductByIdAsync(i.ProductId);
                if (responseIdProduct!=null && responseIdProduct.IsSuccess)
                {
                    ProductModel product = JsonConvert.DeserializeObject<ProductModel>(responseIdProduct.Result.ToString());
                    if (product != null && product.WinrarPassword != null)
                    {
                        await _helperService.SendMail(new SendMailModel()
                        {
                            Email = email,
                            Subject = $"Password of {product.Name}",
                            Body = $"This is password of {product.Name} \n {product.WinrarPassword}"
                        });
                    }
                }
            }

            await _helperService.SendMail(new SendMailModel()
            {
                Email = email,
                Subject = "Payment Success",
                Body = $"Your payment for order {orderId} is successful. Thank you for shopping with us."
            });

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> PaymentSuccessPayOs(
     string? code,
        string? id,
         bool? cancel,
       string? status,
         long? orderCode)
        {
            if (orderCode == null)
            {
                return RedirectToAction(nameof(PaymentFailure));
            }

            string orderId = HttpContext.Request.Cookies["orderId"];
            HttpContext.Response.Cookies.Delete("orderId");

            ResponseModel response = await _paymentService.Paid(new PaidModel()
            {
                OrderId = orderId,
                TransactionId = orderCode.ToString(),
                Status = new PaymentStatusModel()
                {
                    OrderStatus = OrderStatus.Paid,
                    PaymentMethod = PaymentMethod.CreditCard,
                    PaymentName = "PayOS",
                    PaymentStatus = PaymentStatus.Paid
                }
            });

            if (!response.IsSuccess)
                return RedirectToAction("PaymentFailure");


            OrderModel payosOrder = JsonConvert.DeserializeObject<OrderModel>(response.Result.ToString());
            PaymentSuccessModel model = new()
            {
                OrderId = orderId,
                Amount = payosOrder.TotalPrice,
                OrderType = "PayOS",
                ResponseTime = ((DateTimeOffset)payosOrder.UpdatedAt).ToUnixTimeMilliseconds(),
            };

            string email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                TempData["error"] = "You should login first";
                return RedirectToAction("Login", "User");
            }
            if(payosOrder == null || payosOrder.Items ==null)
            {
                TempData["error"] = "Order not found";
                return RedirectToAction("PaymentFailure");
            }
            foreach (var i in payosOrder.Items)
            {
                ResponseModel responseIdProduct = await _productService.GetProductByIdAsync(i.ProductId);
                if (responseIdProduct!=null && responseIdProduct.IsSuccess)
                {
                    ProductModel product = JsonConvert.DeserializeObject<ProductModel>(responseIdProduct.Result.ToString());
                    if (product != null && product.WinrarPassword!=null)
                    {
                        await _helperService.SendMail(new SendMailModel()
                        {
                            Email = email,
                            Subject = $"Password of {product.Name}",
                            Body = $"This is password of {product.Name} \n {product.WinrarPassword}"
                        });
                    }
                }
            }

            await _helperService.SendMail(new SendMailModel()
            {
                Email = email,
                Subject = "Payment Success",
                Body = $"Your payment for order {orderId} is successful. Thank you for shopping with us."
            });


            return View("PaymentSuccess", model);
        }


        public IActionResult PaymentFailure()
        {
            return View();
        }

        #endregion

    }
}
