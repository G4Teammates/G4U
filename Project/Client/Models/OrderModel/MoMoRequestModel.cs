using System.ComponentModel.DataAnnotations;
using Xunit.Abstractions;

namespace Client.Models.OrderModel
{
    public class MoMoRequestModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Amount is required.")]
        [Range(1000, 50000000, ErrorMessage = "Amount must be between 1000 and 50,000,000.")]
        public long Amount { get; set; }

        public MoMoRequestModel(string id, long amount)
        {
            Id = id;
            Amount = amount;
        }

        public MoMoRequestModel()
        {
        }

        public MoMoRequestModel(OrderModel model)
        {
            Id = model.Id;
            Amount = (long)model.TotalPrice;
        }
    }
}
