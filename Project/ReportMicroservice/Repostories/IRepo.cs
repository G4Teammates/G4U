using ReportMicroservice.Models;
using System.Threading.Tasks;

namespace ReportMicroservice.Repostories
{
    public interface IRepo
    {
        Task<ResponseDTO> GetAll(int page, int pageSize);
        Task<ResponseDTO> CreateReport(CreateReportsModels model, string UserName);
        Task<ResponseDTO> UpdateReport(string reportId, int status);
    }
}
