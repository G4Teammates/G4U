using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.Models;
namespace UserMicroservice.Repositories.IRepositories
{
    public interface IUserService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetUser(Guid id);
        Task<ResponseModel> AddUser(UserModel user);
        Task<ResponseModel> UpdateUser(UserModel user);
        Task<ResponseModel> DeleteUser(Guid id);
        Task<ResponseModel>? FindUsers(string? query);


        //Task<ICollection<UserModel>> FindUsersCriteria(SearchCriteria criteria);


    }

}
