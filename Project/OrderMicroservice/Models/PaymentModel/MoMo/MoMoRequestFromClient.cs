using System.ComponentModel.DataAnnotations;

namespace OrderMicroservice.Models.PaymentModel.MoMo
{
    public class MoMoRequestFromClient
    {
        public string Id {  get; set; }
        [Required(ErrorMessage = "Amount is required.")]
        [Range(1000, 50000000, ErrorMessage = "Amount must be between 1000 and 50,000,000.")]
        public long Amount { get; set; }
    }

}

