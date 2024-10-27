using OrderMicroservice.Models;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponseModel> MoMoPayment(string orderId, long amount);
        //Task<ResponseModel> VierQRPayment(string orderIdAsString, int amount, string productName, int quantity);
    }
}
