using Client.Models;
using Client.Models.UserDTO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Client.Models.Enum.UserEnum.User;

namespace Client.Repositories.Interfaces.User
{
    public interface IUserService
    {
        Task<ResponseModel> GetAllUserAsync(int? pageNumber, int pageSize);
        Task<ResponseModel> CreateUserAsync(CreateUser user);
        Task<ResponseModel> GetUserAsync(string id);
        Task<ResponseModel> UpdateUser(UpdateUser user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query, int? pageNumber, int pageSize);
        Task<ResponseModel> ChangeStatus(string id, UserStatus status);
        Task<ResponseModel> GetAllProductsInWishList(string id);
        Task<ResponseModel> AddToWishList(WishlistModel WishlistModel, string userName);
        Task<ResponseModel> RemoveWishList(string productId, string userName);
    }
}
