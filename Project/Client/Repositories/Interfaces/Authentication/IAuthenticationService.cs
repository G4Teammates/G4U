using Client.Models;
using Client.Models.AuthenModel;

namespace Client.Repositories.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        public Task<ResponseModel> LoginAsync(LoginRequestModel loginModel);
        public Task<ResponseModel> RegisterAsync(RegisterModel user);
        public Task<ResponseModel> ChangePasswordAsync(string username, string oldPassword, string newPassword);
        public Task<ResponseModel> ResetPasswordAsync(string username, string newPassword);
        public Task<ResponseModel> ForgotPasswordAsync(string username);
        public Task<ResponseModel> LogoutAsync();

    }
}
