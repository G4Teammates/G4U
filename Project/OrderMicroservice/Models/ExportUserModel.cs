using OrderMicroservice.Models.UserModel.Enum;

namespace OrderMicroservice.Models
{
    public class ExportUserModel
    {
        public string Id { get; set; }
        public string PublisherName { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalPrice { get; set; }
        public BankName BankName { get; set; }
        public string BankAccount { get; set; }
        public string Email {  get; set; }
        public string Phone { get; set; }
        public decimal ProfitOfMonth { get; set; }
        public decimal OriginalPriceOfMonth { get; set; }
    }
}
