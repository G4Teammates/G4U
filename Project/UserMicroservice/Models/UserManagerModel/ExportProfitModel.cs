using UserMicroservice.DBContexts.Enum;

namespace UserMicroservice.Models.UserManagerModel
{
    public class ExportProfitModel
    {
        public string Id { get; set; } // Id của publisher
        public string PublisherName { get; set; } // Tên publisher
        public BankName BankName { get; set; } // Tên ngân hàng
        public string BankAccount { get; set; } // Tài khoản ngân hàng
        public string Email { get; set; } // Email liên hệ
        public string Phone { get; set; } // Số điện thoại liên hệ
        public decimal ProfitOfMonth { get; set; } // Lợi nhuận tháng này
        public decimal OriginalPriceOfMonth { get; set; } // Tổng giá trị gốc trong tháng
        public decimal TotalProfit { get; set; } // Tổng lợi nhuận
    }

}
