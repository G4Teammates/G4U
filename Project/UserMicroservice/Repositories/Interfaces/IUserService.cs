using MongoDB.Bson;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.Models;
namespace UserMicroservice.Repositories.IRepositories
{
    public interface IUserService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetUser(string id);
        Task<ResponseModel> AddUserAsync(AddUserModel user);
        Task<ResponseModel> UpdateUser(UserUpdate user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query);


        //Task<ICollection<UserModel>> FindUsersCriteria(SearchCriteria criteria);


    }

}
