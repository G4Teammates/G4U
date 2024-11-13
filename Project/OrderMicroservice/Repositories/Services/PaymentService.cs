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

namespace OrderMicroservice.Repositories.Services
{
    public class PaymentService(IOrderService orderService) : IPaymentService
    {
        private readonly IOrderService _orderService = orderService;
        private static readonly HttpClient client = new();
        private static readonly string Gateway = "https://localhost:7296";
        private static readonly string MoMoGateway = "https://test-payment.momo.vn/v2/gateway/api/create";
        private static readonly string IpnMomo = "https://5960-2402-800-63b6-b08e-5931-a421-6cbb-8c20.ngrok-free.app" + "/api/payment/ipn/momo";

        public async Task<ResponseModel> MoMoPayment(string id, long amount)
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
                    redirectUrl = $"{Gateway}/Order/PaymentSuccess",
                    ipnUrl = IpnMomo,
                    amount = amount,
                    orderId = id,
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

        public async Task<ResponseModel> VierQRPayment(string id, int amount, ICollection<OrderItemModel> items)
        {

            ResponseModel response = new();
            try
            {
                PayOS payOS = new PayOS(PayOSOptionModel.ClientId!, PayOSOptionModel.ApiKey!, PayOSOptionModel.ChecksumKey!);
                Random random = new Random();
                long orderId = ((long)random.Next(int.MinValue, int.MaxValue) << 32) | (long)random.Next(int.MinValue, int.MaxValue);

                List<ItemData> itemData = items.Select(i => new ItemData(i.ProductName, i.Quantity, (int)i.Price)).ToList();

                PaymentData paymentData = new PaymentData(orderId, amount, $"Payment for order: {id}",
                     itemData, cancelUrl: $"{Gateway}/Order/PaymentFailure", returnUrl: $"{Gateway}/Order/PaymentSuccess");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                response.Result = new VietQRResponse
                {
                    CheckoutUrl = createPayment.checkoutUrl,
                    PaymentTransactionId = orderId.ToString()
                };
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

                ResponseModel orderResult = await _orderService.GetOrderById(request.OrderId);

                OrderModel order = (OrderModel)orderResult.Result;
                //MoMoSignature orderSignature = new MoMoSignature()
                //{
                //    AccessKey = accessKey,
                //    Amount = (long)order.TotalPrice,
                //    PartnerCode = "MOMO",
                //    OrderId = order.Id
                //};

                //string signature = GenarateSignatureResponseMoMo(orderSignature, secretKey);

                if (request.Amount == order.TotalPrice && request.PartnerCode == "MOMO" && request.OrderId == order.Id)
                {
                    response.IsSuccess = true;
                    response.Message = "IPN signature MoMo is valid";
                    response = await Paid(new PaidModel
                    {
                        OrderId = request.OrderId,
                        TransactionId = request.TransId.ToString()
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
                ResponseModel findOrder = await _orderService.GetOrderById(model.OrderId);
                if (!findOrder.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = "Order not found";
                    return response;
                }

                ResponseModel updateTransId = await _orderService.UpdateTransId(model.OrderId, model.TransactionId);
                if (!updateTransId.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to update transaction id";
                    return response;
                }

                ResponseModel updateStatus = await _orderService.UpdateStatus(model.OrderId, new PaymentStatusModel
                {
                    PaymentStatus = PaymentStatus.Paid,
                    OrderStatus = OrderStatus.Paid,
                    PaymentMethod = PaymentMethod.Wallet,
                    PaymentName = "MoMo"
                    
                });

                if (!updateStatus.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to update order status";
                    return response;
                }
                response.IsSuccess = true;
                response.Message = "Order paid successfully";


            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }
            return response;
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