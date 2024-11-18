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
            // 1. Deserialize cart và kiểm tra null
            CartModel cart;
            try
            {
                cart = JsonConvert.DeserializeObject<CartModel>(orderJson);
            }
            catch (JsonException ex)
            {
                TempData["Error"] = "Invalid cart data";
                return RedirectToAction("Index", "Cart");
            }


            if (cart == null || cart.Order == null || cart.Order.Items == null || !cart.Order.Items.Any())
            {

                TempData["Error"] = "Cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            // Gán phương thức thanh toán và tên mặc định
            cart.PaymentMethod = paymentMethod;
            cart.Order.PaymentName = "Pending";

            // 2. Xử lý đơn hàng miễn phí
            if (cart.Order.TotalPrice == 0)
            {
                await HandleFreeOrder(cart);
                TempData["Success"] = "Free order completed successfully";
                if (HttpContext.Request.Cookies.ContainsKey("cart"))
                {
                    HttpContext.Response.Cookies.Delete("cart");
                }

                return RedirectToAction("Index", "Home");
            }

            // 3. Xử lý các phương thức thanh toán khác
            switch (cart.PaymentMethod)
            {
                case Models.Enum.OrderEnum.PaymentMethod.Wallet:
                    return await HandleMoMoPayment(cart);

                case Models.Enum.OrderEnum.PaymentMethod.CreditCard:
                    // Gọi hàm xử lý CreditCard
                    return await HandleCreditCardPayment(cart);

                default:
                    TempData["Error"] = "Unsupported payment method";
                    return RedirectToAction("Index", "Cart");
            }
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



        private async Task HandleFreeOrder(CartModel cart)
        {
            // Lấy danh sách sản phẩm miễn phí
            List<string> productsIdFree = cart.Order.Items.Select(i => i.ProductId).ToList();
            cart.Order.PaymentName = "Free";

            PaymentStatusModel paymentStatusFree = new()
            {
                OrderStatus = OrderStatus.Paid,
                PaymentStatus = PaymentStatus.Paid
            };

            foreach (var productId in productsIdFree)
            {
                var response = await _orderService.UpdateStatus(productId, paymentStatusFree);
                if (!response.IsSuccess)
                {
                    TempData["Error"] = $"Failed to update status for product {productId}";
                    return;
                }
            }

        }


        private async Task<IActionResult> HandleMoMoPayment(CartModel cart)
        {
            // Tạo đơn hàng
            CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
            ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);

            if (!responseCreateOrder.IsSuccess)
            {
                TempData["Error"] = "Failed to create order";
                return RedirectToAction("PaymentFailure");
            }

            // Thanh toán qua MoMo
            OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
            MoMoRequestModel request = new MoMoRequestModel(newOrder.Id, (long)cart.Order.TotalPrice);
            ResponseModel responsePayment = await _paymentService.MoMoPayment(request);

            if (!responsePayment.IsSuccess)
            {
                TempData["Error"] = "Payment through MoMo failed";
                return RedirectToAction("PaymentFailure");
            }

            // Xóa giỏ hàng và chuyển hướng đến trang MoMo
            if (HttpContext.Request.Cookies.ContainsKey("cart"))
            {
                HttpContext.Response.Cookies.Delete("cart");
            }

            return Redirect(responsePayment.Result.ToString());
        }


        private async Task<IActionResult> HandleCreditCardPayment(CartModel cart)
        {
            // Thêm logic xử lý thanh toán qua Credit Card ở đây
            TempData["Info"] = "Credit card payment is not implemented yet";
            return RedirectToAction("Index", "Cart");
        }

    }
}
