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
        }        //public async Task<IActionResult> Checkout(string orderJson, PaymentMethod paymentMethod)
        //{
        //    CartModel cart = JsonConvert.DeserializeObject<CartModel>(orderJson);
        //    cart.PaymentMethod = paymentMethod;
        //    cart.Order.PaymentName = "Pending";
        //    if (cart == null)
        //    {
        //        TempData["Error"] = "Cart is empty";
        //        return RedirectToAction("Index", "Cart");
        //    }
        //    if (cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.Wallet)
        //    {
        //        CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
        //        ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);
        //        if (responseCreateOrder.IsSuccess)
        //        {
        //            OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
        //            MoMoRequestModel request = new MoMoRequestModel(newOrder.Id, (long)cart.Order.TotalPrice);
        //            ResponseModel responsePayment = await _paymentService.MoMoPayment(request);
        //            if (responsePayment.IsSuccess)
        //            {
        //                HttpContext.Response.Cookies.Delete("cart");
        //                return Redirect(responsePayment.Result.ToString());
        //            }
        //            else
        //            {
        //                return RedirectToAction("PaymentFailure");
        //            }
        //        }
        //    }

        //    else if (cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.CreditCard)
        //    {

        //    }
        //    else if (cart.Order.TotalPrice == 0)
        //    {
        //        CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(cart.Order);
        //        ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);
        //        if (responseCreateOrder.IsSuccess)
        //        {
        //            OrderModel newOrder = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());
        //            PaymentStatusModel status = new PaymentStatusModel()
        //            {
        //                OrderStatus = OrderStatus.Paid,
        //                PaymentMethod = PaymentMethod.Free,
        //                PaymentName = "Free",
        //                PaymentStatus = PaymentStatus.Paid
        //            };
        //            ResponseModel responseUpdateOrder = await _orderService.UpdateStatus(newOrder.Id, status);
        //            OrderModel freeOrder = JsonConvert.DeserializeObject<OrderModel>(responseUpdateOrder.Result.ToString());
        //            return RedirectToAction("PaymentSuccess/OrderModel", "Order", freeOrder); // Thay "YourControllerName" bằng tên của controller chứa phương thức này.
        //        }

        //        else
        //        {

        //        }

        //        return RedirectToAction("PaymentFailure");
        //    }
        //    return RedirectToAction("PaymentFailure");
        //}


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
            }
            else if (cart.PaymentMethod == Models.Enum.OrderEnum.PaymentMethod.CreditCard)
            {
                // Handle credit card payment here
            }
            else if (cart.Order.TotalPrice == 0)
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
                else
                {
                    return RedirectToAction("PaymentFailure");
                }
            }

            return RedirectToAction("PaymentFailure");
        }

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



        //private async Task HandleFreeOrder(CartModel cart)
        //{
        //    // Lấy danh sách sản phẩm miễn phí
        //    List<string> productsIdFree = cart.Order.Items.Select(i => i.ProductId).ToList();
        //    cart.Order.PaymentName = "Free";

        //    PaymentStatusModel paymentStatusFree = new()
        //    {
        //        OrderStatus = OrderStatus.Paid,
        //        PaymentStatus = PaymentStatus.Paid,
        //        PaymentMethod = PaymentMethod.Free,
        //        PaymentName = "Free"
        //    };

        //    foreach (var productId in productsIdFree)
        //    {
        //        var response = await _orderService.UpdateStatus(productId, paymentStatusFree);
        //        if (!response.IsSuccess)
        //        {
        //            TempData["Error"] = $"Failed to update status for product {productId}";
        //            return;
        //        }
        //    }

        //}


        //private async Task<IActionResult> HandleMoMoPayment(OrderModel newOrder)
        //{
        //    // Tạo đơn hàng
        //    CreateOrderModel createOrder = _mapper.Map<CreateOrderModel>(newOrder);
        //    ResponseModel responseCreateOrder = await _orderService.CreateOrder(createOrder);
        //    OrderModel order = JsonConvert.DeserializeObject<OrderModel>(responseCreateOrder.Result.ToString());

        //    if (!responseCreateOrder.IsSuccess)
        //    {
        //        TempData["Error"] = "Failed to create order";
        //        return RedirectToAction("PaymentFailure");
        //    }
        //    // Thanh toán qua MoMo
        //    //OrderModel newOrder = cart.Order;
        //    MoMoRequestModel request = new MoMoRequestModel(order.Id, (long)order.TotalPrice);
        //    ResponseModel responsePayment = await _paymentService.MoMoPayment(request);

        //    if (!responsePayment.IsSuccess)
        //    {
        //        TempData["Error"] = "Payment through MoMo failed";
        //        return RedirectToAction("PaymentFailure");
        //    }

        //    // Xóa giỏ hàng và chuyển hướng đến trang MoMo
        //    if (HttpContext.Request.Cookies.ContainsKey("cart"))
        //    {
        //        HttpContext.Response.Cookies.Delete("cart");
        //    }

        //    return Redirect(responsePayment.Result.ToString());
        //}


        //private async Task<IActionResult> HandleCreditCardPayment(OrderModel cart)
        //{
        //    // Thêm logic xử lý thanh toán qua Credit Card ở đây
        //    TempData["Info"] = "Credit card payment is not implemented yet";
        //    return RedirectToAction("Index", "Cart");
        //}

    }
}
