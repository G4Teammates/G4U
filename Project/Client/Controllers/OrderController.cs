using AutoMapper;
using Client.Models;
using Client.Models.Enum.OrderEnum;
using Client.Models.OrderModel;
using Client.Repositories.Interfaces.Order;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace Client.Controllers
{
    public class OrderController(IOrderService orderService, IPaymentService paymentService, IMapper mapper) : Controller
    {
        private readonly IMapper _mapper = mapper;
        private readonly IOrderService _orderService = orderService;
        private readonly IPaymentService _paymentService = paymentService;
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
            CartModel cart = JsonConvert.DeserializeObject<CartModel>(orderJson);
            cart.PaymentMethod = paymentMethod;
            cart.Order.PaymentName = "Pending";

            if (cart == null)
            {
                TempData["Error"] = "Cart is empty";
                return RedirectToAction("Index", "Cart");
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
            // Xử lý thanh toán bằng thẻ tín dụng
            // Bạn có thể thêm logic thanh toán thẻ tín dụng ở đây
            // Ví dụ: Gửi thông tin thanh toán đến dịch vụ thẻ tín dụng

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
                OrderModel freeOrder = JsonConvert.DeserializeObject<OrderModel>(responseUpdateOrder.Result.ToString());

                return RedirectToAction("PaymentSuccess", "Order", new
                {
                    orderId = newOrder.Id,
                    amount = 0,
                    orderType = "Free",
                    responseTime = ((DateTimeOffset)newOrder.UpdatedAt).ToUnixTimeMilliseconds()
                });
            }

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

            return View(model);
        }


        public IActionResult PaymentFailure()
        {
            return View();
        }

        #endregion

    }
}
