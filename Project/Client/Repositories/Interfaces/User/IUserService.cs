using Client.Models;
using Client.Models.UserDTO;

namespace Client.Repositories.Interfaces.User
{
    public interface IUserService
    {
        Task<ResponseModel> GetAllUserAsync();
        Task<ResponseModel> CreateUserAsync(CreateUser user);
        Task<ResponseModel> GetUserAsync(string id);
        Task<ResponseModel> UpdateUser(UpdateUser user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query);
    }
}
