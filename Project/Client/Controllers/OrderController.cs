using AutoMapper;
using Client.Models;
using Client.Models.Enum.OrderEnum;
using Client.Models.OrderModel;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Order;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace Client.Controllers
{
    public class OrderController(IOrderService orderService, IPaymentService paymentService, IMapper mapper, IHelperService helperService, ITokenProvider tokenProvider) : Controller
    {
        #region declare
        private readonly IMapper _mapper = mapper;
        private readonly IOrderService _orderService = orderService;
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IHelperService _helperService = helperService;
        private readonly ITokenProvider _tokenProvider = tokenProvider; 
        private string JWT = "JWT";
        private string IsLogin = "IsLogin";
        private string RememberMe = "RememberMe";
        private string Cart = "cart";
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
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
                isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin,false));
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
                TempData["orderId"] = newOrder.Id;
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
        public IActionResult PaymentSuccess(string? partnerCode, string? orderId, string? requestId, decimal amount, string? orderInfo, string? orderType, string? transId, int? resultCode, string message, string payType, long responseTime, string extraData, string signature)
        {
            var model = new PaymentSuccessModel
            {
                OrderId = orderId,
                Amount = amount,
                OrderType = orderType,
                ResponseTime = responseTime,
            };
            string email = User.FindFirst(ClaimTypes.Email)?.Value;
            _helperService.SendMail(new SendMailModel()
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
            string orderId = TempData["orderId"].ToString();
            if(orderCode == null)
            {
                return RedirectToAction(nameof(PaymentFailure));
            }
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
