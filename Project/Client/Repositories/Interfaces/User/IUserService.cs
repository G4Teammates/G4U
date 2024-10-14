using Client.Models;
using Client.Models.UserDTO;

namespace Client.Repositories.Interfaces.User
{
    public interface IUserService
    {
        Task<ResponseModel> GetAllUserAsync();
        Task<ResponseModel> CreateUserAsync(UsersDTO user);
        Task<ResponseModel> GetUserAsync(string id);
        Task<ResponseModel> UpdateUser(string id, UpdateUser user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query);
    }
}
