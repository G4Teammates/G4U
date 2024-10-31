using Client.Models;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.User;
using Client.Utility;
using System.Net.Http;
using System.Text;


namespace Client.Repositories.Services.User
{
    public class UserService : IUserService
    {
        public readonly IBaseService _baseService;
        public UserService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseModel> CreateUserAsync(CreateUser user)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = user,
                Url = StaticTypeApi.APIGateWay + "/User"
            });
        }

        public async Task<ResponseModel> DeleteUser(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/User/delete/" + id
            });
        }

        public async Task<ResponseModel> FindUsers(string? query)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + $"/User/search?query={query}"
            });
        }


        public async Task<ResponseModel> GetAllUserAsync()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/User"
            });
        }

        public async Task<ResponseModel> GetUserAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/User/" + id
            });
        }

        public async Task<ResponseModel> UpdateUser(UpdateUser user)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = user,
                Url = StaticTypeApi.APIGateWay + "/User"
            });
        }
    }
}
