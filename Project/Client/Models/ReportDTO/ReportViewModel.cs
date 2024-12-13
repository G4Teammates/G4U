using Client.Models.ComentDTO;

namespace Client.Models.ReportDTO
{
    public class ReportViewModel
    {
        public CreateReportsModels? CreateReport { get; set; }

        public ICollection<ReportsModel>? Report { get; set; }

        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public int pageCount { get; set; }
    }
}
