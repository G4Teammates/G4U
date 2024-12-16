using OrderMicroservice.Models;
using OrderMicroservice.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;
using OrderMicroservice.Models.PaymentModel;
using Newtonsoft.Json;
using Net.payOS.Types;
using Net.payOS;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel.MoMo;
using OrderMicroservice.DBContexts.Enum;
using OrderMicroservice.Models.Message;
using OrderMicroservice.Models.PaymentModel.PayOsModel;
using RabbitMQ.Client;

namespace OrderMicroservice.Repositories.Services
{
    public class PaymentService(IOrderService orderService, IMessage message, IHelperService helperService) : IPaymentService
    {
        private readonly IOrderService _orderService = orderService;
        private readonly IHelperService _helperService = helperService;
        private static readonly HttpClient client = new();

        private static readonly string MoMoGateway = "https://test-payment.momo.vn/v2/gateway/api/create";

        //Host
        //private static readonly string ClientUrl = "https://webmvc-gbfngyfng2bfbccj.southeastasia-01.azurewebsites.net";
        //private static readonly string IpnMomo = "https://oderapi-fkddgtb7ayeweyab.southeastasia-01.azurewebsites.net" + "/api/payment/ipn/momo";    

        //Local
        private static readonly string ClientUrl = "https://webmvc-gbfngyfng2bfbccj.southeastasia-01.azurewebsites.net";
        private static readonly string IpnMomo = " https://8df7-2402-800-63b6-c615-1a5-b818-834e-68c7.ngrok-free.app" + "/api/payment/ipn/momo";

        private IMessage _message = message;
        public async Task<ResponseModel> MoMoPayment(MoMoRequestFromClient requestClient)
        {
            ResponseModel response = new();
            string requestId = Guid.NewGuid().ToString();
            try
            {
                string accessKey = MoMoOptionModel.AccessKey!;
                string secretKey = MoMoOptionModel.SecretKey!;

                MoMoRequest request = new MoMoRequest
                {
                    orderInfo = "Pay with MoMo",
                    partnerCode = "MOMO",
                    redirectUrl = $"{ClientUrl}/Order/PaymentSuccess",
                    ipnUrl = IpnMomo,
                    amount = requestClient.Amount,
                    orderId = requestClient.Id,
                    requestId = requestId,
                    extraData = "",
                    partnerName = "MoMo Payment",
                    storeId = "G4T Store",
                    orderGroupId = "",
                    autoCapture = true,
                    lang = "vi",
                    requestType = "captureWallet",
                };

                request.signature = GenarateSignatureRequestMoMo(request, secretKey, accessKey);

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var quickPayResponse = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);

                var contents = await quickPayResponse.Content.ReadAsStringAsync();

                // Deserialize JSON response to MoMoResponse
                MoMoResponse? parsedResponse = JsonConvert.DeserializeObject<MoMoResponse>(contents);
                if (parsedResponse != null)
                {
                    response.Result = parsedResponse.payUrl;
                    response.IsSuccess = parsedResponse.resultCode == 0;
                    response.Message = parsedResponse.message;
                }
                else
                {
                    response.Result = "Invalid response data";
                    response.IsSuccess = false;
                    response.Message = "Failed to parse response from MoMo";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel> VierQRPayment(VietQRRequest request)
        {
            ResponseModel response = new();
            try
            {
                PayOS payOS = new PayOS(PayOSOptionModel.ClientId!, PayOSOptionModel.ApiKey!, PayOSOptionModel.ChecksumKey!);
                Random random = new Random();
                int orderId = random.Next(1, int.MaxValue);

                List<ItemData> itemData = request.Items.Select(i => new ItemData(i.ProductName, i.Quantity, (int)i.Price)).ToList();

                PaymentData paymentData = new PaymentData(orderId, (int)request.Amount, $"Payment with G4T",
                     itemData, cancelUrl: $"{ClientUrl}/Order/PaymentFailure", returnUrl: $"{ClientUrl}/Order/PaymentSuccessPayOs");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                response.Result = createPayment.checkoutUrl;
                response.Message = "Create VietQR payment success";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }
            return response;
        }

        public async Task<ResponseModel> IpnMoMo(MoMoIPNResquest request)
        {
            ResponseModel response = new();
            try
            {
                string accessKey = MoMoOptionModel.AccessKey!;
                string secretKey = MoMoOptionModel.SecretKey!;

                // 1. Lấy thông tin order
                ResponseModel responseFindOrder = await _orderService.GetOrderById(request.OrderId, 1, 1);
                var pagedOrders = responseFindOrder.Result as X.PagedList.IPagedList<OrderModel>;
                if (pagedOrders == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Failed to cast Result to IPagedList<OrderModel>."
                    };
                }

                // Nếu cần chuyển đổi sang ICollection<OrderModel>
                ICollection<OrderModel> orders = pagedOrders.ToList();

                var order = orders.FirstOrDefault();



                if (request.Amount == order.TotalPrice && request.PartnerCode == "MOMO" && request.OrderId == order.Id)
                {
                    response.IsSuccess = true;
                    response.Message = "IPN signature MoMo is valid";
                    response = await Paid(new PaidModel
                    {
                        OrderId = request.OrderId,
                        TransactionId = request.TransId.ToString(),
                        Status = new PaymentStatusModel()
                        {
                            OrderStatus = OrderStatus.Paid,
                            PaymentMethod = PaymentMethod.Wallet,
                            PaymentName = "MoMo",
                            PaymentStatus = PaymentStatus.Paid
                        }
                    }); 
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "IPN signature MoMo is invalid";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }
            return response;
        }

        public async Task<ResponseModel> Paid(PaidModel model)
        {
            ResponseModel response = new();

            try
            {
                // 1. Lấy thông tin order
                ResponseModel responseFindOrder = await _orderService.GetOrderById(model.OrderId, 1, 1);
                var pagedOrders = responseFindOrder.Result as X.PagedList.IPagedList<OrderModel>;
                if (pagedOrders == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Failed to cast Result to IPagedList<OrderModel>."
                    };
                }

                // Nếu cần chuyển đổi sang ICollection<OrderModel>
                ICollection<OrderModel> orders = pagedOrders.ToList();

                var order = orders.FirstOrDefault();
                // 2. Cập nhật TransactionId
                var updateTransIdResponse = await _orderService.UpdateTransId(model.OrderId, model.TransactionId);
                if (!updateTransIdResponse.IsSuccess)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = $"Failed to update transaction ID for OrderId: {model.OrderId}"
                    };
                }

                // 3. Cập nhật trạng thái thanh toán
                var updateStatusResponse = await _orderService.UpdateStatus(model.OrderId, model.Status);

                if (!updateStatusResponse.IsSuccess)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = $"Failed to update payment status for OrderId: {model.OrderId}"
                    };
                }

                // 4. Cập nhật số lượng sản phẩm đã bán
                var updateSoldResponse = await UpdateSold(new ProductSoldRequest
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

                // 5. Hoàn tất thanh toán
                response.IsSuccess = true;
                response.Message = "Order paid successfully";
                response.Result = order;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                response.IsSuccess = false;
                response.Message = $"Unexpected error: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel> SendNotification(SendMailModel model)
        {
            try
            {
                await _helperService.SendEmailAsync(model.Email, model.Subject, model.Body);
                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Send notification success"
                };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = $"Failed to send notification: {ex.Message}"
                };
            }
        }






        public async Task<ResponseModel> UpdateSold(ProductSoldRequest request)
        {
            try
            {
                _message.SendingMessage2(request, "Product", "order_for_sold_product", "order_for_sold_product", ExchangeType.Direct, true, false, false, false);
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





        private string GenarateSignatureRequestMoMo(MoMoRequest signature, string secretKey, string accessKey)
        {
            // Tạo chuỗi `rawHash` từ các giá trị cần có trong chữ ký
            string rawHash = "accessKey=" + accessKey +
                 "&amount=" + signature.amount +
                 "&extraData=" + (signature.extraData ?? "") +
                 "&ipnUrl=" + signature.ipnUrl +
                 "&orderId=" + signature.orderId +
                 "&orderInfo=" + signature.orderInfo +
                 "&partnerCode=" + signature.partnerCode +
                 "&redirectUrl=" + signature.redirectUrl +
                 "&requestId=" + signature.requestId +
                 "&requestType=" + signature.requestType;

            return HashSHA256(rawHash, secretKey);

        }

        private string GenarateSignatureResponseMoMo(MoMoSignature signature, string secretKey)
        {
            // Tạo chuỗi `rawHash` từ các giá trị cần có trong chữ ký
            string rawHash = "accessKey=" + signature.AccessKey +
                             "&amount=" + signature.Amount +
                             "&orderId=" + signature.OrderId +
                             "&partnerCode=" + signature.PartnerCode;
            return HashSHA256(rawHash, secretKey);
        }

        private string HashSHA256(string rawHash, string secretKey)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            // Chuyển đổi chuỗi `rawHash` và `secretKey` thành mảng byte
            byte[] textBytes = encoding.GetBytes(rawHash);
            byte[] keyBytes = encoding.GetBytes(secretKey);

            byte[] hashBytes;

            // Sử dụng HMACSHA256 để tính chữ ký
            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            // Trả về chuỗi `signature` dưới dạng hexadecimal, tất cả chữ thường
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

    }
}