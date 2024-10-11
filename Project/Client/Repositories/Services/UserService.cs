using Client.Models;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.User;
using Client.Utility;


namespace Client.Repositories.Services
{
    public class UserService : IUserService
    {
        public readonly IBaseService _baseService;
        public UserService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseModel> CreateUserAsync(UsersDTO user)
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
                Url = StaticTypeApi.APIGateWay + "/User/" + id
            });
        }

        public Task<ResponseModel>? FindUsers(string? query)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel?> GetAllUserAsync()
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

        public async Task<ResponseModel> UpdateUser(string id,UpdateUser user)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = user,
                Url = StaticTypeApi.APIGateWay + "/User" + id
            });
        }
    }
}
