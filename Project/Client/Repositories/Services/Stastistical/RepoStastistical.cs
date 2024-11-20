using Client.Models;
using Client.Models.Statistical;
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
            Console.WriteLine("stastistical getall is running");
            return await _baseService.SendAsync(new RequestModel()
                {
                    ApiType = StaticTypeApi.ApiType.GET,
                    Url = StaticTypeApi.APIGateWay + "/Statistical?page=" + page.ToString() + "&pageSize=" + pageSize.ToString()
                });              
        }
        public async Task<ResponseModel?> GetByUser(TotalGroupByUserRequest totalGroupByUserRequest)
        {
            // Xây dựng query string từ các tham số của `totalGroupByUserRequest`
            var queryParams = $"?UserName={totalGroupByUserRequest.UserName}&CreateAt={totalGroupByUserRequest.CreateAt:yyyy-MM-dd}";

            Console.WriteLine("stastistical getall is running");
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Statistical/GetStastisticalByUser" + queryParams
            });
        }
    }
}
