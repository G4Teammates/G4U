using AutoMapper;
using Client.Models;
using Client.Models.Enum.OrderEnum;
using Client.Models.OrderModel;
using Client.Repositories.Interfaces.Order;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
                ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);
                if (responseCreateOrder.IsSuccess)
                {
                    OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
                    MoMoRequestModel request = new MoMoRequestModel(newOrder.Id, (long)cart.Order.TotalPrice);
                    ResponseModel responsePayment = await _paymentService.MoMoPayment(request);
                    if(responsePayment.IsSuccess)
                    {
                        HttpContext.Response.Cookies.Delete("cart");
                        return Redirect(responsePayment.Result.ToString());
                    }
                    else
                    {
                        return RedirectToAction("PaymentFailure");
                    }
                }
            }

            else if(cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.CreditCard)
            {

            }
            else
            {

            }

            return View();
        }

        public IActionResult PaymentSuccess(string partnerCode, string orderId, string requestId, decimal amount, string orderInfo, string orderType, string transId, int resultCode, string message, string payType, long responseTime, string extraData, string signature)
        {
            var model = new PaymentSuccessModel
            {
                PartnerCode = partnerCode,
                OrderId = orderId,
                RequestId = requestId,
                Amount = amount,
                OrderInfo = orderInfo,
                OrderType = orderType,
                TransId = transId,
                ResultCode = resultCode,
                Message = message,
                PayType = payType,
                ResponseTime = responseTime,
                ExtraData = extraData,
                Signature = signature
            };

            return View(model);
        }

        public IActionResult PaymentFailure()
        {
            return View();
        }
    }
}
