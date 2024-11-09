using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OrderMicroservice.Models;
using OrderMicroservice.Repositories.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Text;
using OrderMicroservice.Models.PaymentModel;
using System.Text.Json;
using Newtonsoft.Json;
using Net.payOS.Types;
using Net.payOS;
using OrderMicroservice.Models.OrderModel;
using Newtonsoft.Json.Converters;

namespace OrderMicroservice.Repositories.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly HttpClient client = new();
        private static readonly string Gateway = "https://localhost:7296";

        public async Task<ResponseModel> MoMoPayment(string orderId, long amount)
        {
            ResponseModel response = new();
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
            try
            {
                string accessKey = MoMoOptionModel.AccessKey!;
                string secretKey = MoMoOptionModel.SecretKey!;

                MoMoRequest request = new MoMoRequest
                {
                    orderInfo = "Pay with MoMo",
                    partnerCode = "MOMO",
                    redirectUrl = "https://",
                    ipnUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b",
                    amount = amount,
                    orderId = orderId,
                    requestId = myuuidAsString,
                    extraData = "",
                    partnerName = "MoMo Payment",
                    storeId = "G4T Store",
                    orderGroupId = "",
                    autoCapture = true,
                    lang = "vi",
                    requestType = "captureWallet",
                };
                request.signature = request.MakeSignature(accessKey, secretKey);

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var quickPayResponse = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);

                var contents = await quickPayResponse.Content.ReadAsStringAsync();

                // Deserialize JSON response to MoMoResponse
                MoMoResponse? parsedResponse = JsonConvert.DeserializeObject<MoMoResponse>(contents);
                if (parsedResponse != null)
                {
                    response.Result = parsedResponse.payUrl;  // Only return payUrl
                    response.IsSuccess = quickPayResponse.IsSuccessStatusCode;
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

        public async Task<ResponseModel> VierQRPayment(OrderModel model)
        {
            string clientId = "e706845a-6c2d-49a1-8d3c-735bbe98df4a";
            string apiKey = "caefb61c-cc8c-4236-bd9d-75b58a606660";
            string checksumKey = "9a1a07556ff46320987f1e13c8f175c52d17fe010739a123cab0451141fc55ad";
            ResponseModel response = new();
            try
            {
                PayOS payOS = new PayOS(clientId, apiKey, checksumKey);
                Random random = new Random();
                long orderId = ((long)random.Next(int.MinValue, int.MaxValue) << 32) | (long)random.Next(int.MinValue, int.MaxValue);

                List<ItemData> items = model.Items.Select(i => new ItemData(i.ProductName, i.Quantity, (int)i.Price)).ToList();

                PaymentData paymentData = new PaymentData(orderId, (int)model.TotalPrice, $"Payment for order: {model.Id}",
                     items, cancelUrl: $"{Gateway}/Order/PaymentFailure", returnUrl: $"{Gateway}/Order/PaymentSuccess");

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
    }
}
