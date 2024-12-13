namespace OrderMicroservice.Models
{
    public class ExportResult
    {
        public ICollection<ExportUserModel> ExportProfits { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
