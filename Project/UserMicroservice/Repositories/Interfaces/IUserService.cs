using MongoDB.Bson;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts.Enum;
using UserMicroservice.Models;
using UserMicroservice.Models.Message;
using UserMicroservice.Models.UserManagerModel;
namespace UserMicroservice.Repositories.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel> GetAll(int pageNumber, int pageSize);
        Task<ResponseModel> GetUser(string id);
        Task<ResponseModel> GetUserByEmail(string email);
        Task<ResponseModel> AddUserAsync(AddUserModel user);
        Task<ResponseModel> UpdateUser(UserUpdate user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query, int page, int pageSize);
        Task<ResponseModel> ChangeStatus(string id, UserStatus status);

        Task<ResponseModel> TotalRequest();

        Task<ResponseModel> GetAllProductsInWishList(string id);
        Task<ResponseModel> AddToWishList(UserWishlistModel userWishlistModel, string userName);

        //Task<ICollection<UserModel>> FindUsersCriteria(SearchCriteria criteria);
        Task<bool> CheckUserByUserNameAsync(string userName);
        Task<ResponseModel> RemoveFromWishList(string productId, string userName);

    }

}
