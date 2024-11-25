using Client.Models;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.User;
using Client.Utility;
using CloudinaryDotNet;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using System.Drawing.Printing;
using System.Net.Http;
using System.Text;
using static Client.Models.Enum.UserEnum.User;


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

        public async Task<ResponseModel> FindUsers(string? query, int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/User/search?query={query}&page={pageNumber}&pageSize={pageSize}"
            });
        }


        public async Task<ResponseModel> GetAllUserAsync(int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/User?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
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
        
        public async Task<ResponseModel> ChangeStatus(string id, UserStatus  status)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = status,
                Url = StaticTypeApi.APIGateWay + "/User/status/" + id
            });
        }

        public async Task<ResponseModel> GetAllProductsInWishList(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/User/getAllProductsInWishList/{id}"
            });
        }

        public async Task<ResponseModel> AddToWishList(WishlistModel WishlistModel, string userName)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = WishlistModel,  
                Url = $"{StaticTypeApi.APIGateWay}/User/addWishList/{userName}"
            });
        }
        public async Task<ResponseModel> RemoveWishList(string productId, string userName)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Url = $"{StaticTypeApi.APIGateWay}/User/removeWishList/{userName}/{productId}"
            });
        }
    }
}
