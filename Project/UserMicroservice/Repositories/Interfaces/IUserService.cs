using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.Models;
namespace UserMicroservice.Repositories.IRepositories
{
    public interface IUserService
    {
        Task<ICollection<UserModel>> GetAll();
        Task<UserModel> GetUser(Guid id);
        Task<UserModel> AddUser(UserModel user);
        Task<UserModel> UpdateUser(UserModel user);
        Task<UserModel> DeleteUser(Guid id);
        Task<ICollection<UserViewModel>>? FindUsers(string? query);


        //Task<ICollection<UserModel>> FindUsersCriteria(SearchCriteria criteria);


    }

}
