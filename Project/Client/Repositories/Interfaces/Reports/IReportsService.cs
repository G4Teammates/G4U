using Client.Models;
namespace Client.Repositories.Interfaces.Reports
{
    public interface IReportsService
    {
        Task<ResponseModel?> GetAll(int page, int pageSize);
        Task<ResponseModel?> CreateReport(CreateReportsModels model, string UserName);
        Task<ResponseModel?> UpdateReport(string reportId, int status);
        Task<ResponseModel?> GetById(string id);
    }
}
