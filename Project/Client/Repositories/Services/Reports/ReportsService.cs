using Client.Models;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Reports;
using Client.Utility;

namespace Client.Repositories.Services.Reports
{
    public class ReportsService : IReportsService
    {
        public readonly IBaseService _baseService;
        public ReportsService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseModel?> CreateReport(CreateReportsModels model, string UserName)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(model.Description), "description");
            formData.Add(new StringContent(model.Related), "related");
            formData.Add(new StringContent(model.Email), "email");
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + $"/Reports/userName/{UserName}"
            });
        }

        public async Task<ResponseModel?> GetAll(int pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Reports?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }
    }
}