using Client.Models;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Stastistical;
using Client.Utility;

namespace Client.Repositories.Services.Stastistical
{
    public class RepoStastistical : IRepoStastistical
    {
        public readonly IBaseService _baseService;
        private readonly HttpClient _httpClient;

        public RepoStastistical(IBaseService baseService, HttpClient httpClient)
        {
            _baseService = baseService;
            _httpClient = httpClient;
        }
        public async Task<ResponseModel?> GetAll(int page, int pageSize)
        {
                return await _baseService.SendAsync(new RequestModel()
                {
                    ApiType = StaticTypeApi.ApiType.GET,
                    Url = StaticTypeApi.APIGateWay + "/Statistical?page=" + page.ToString() + "&pageSize=" + pageSize.ToString()
                });
        
        }
    }
}
