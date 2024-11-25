using Client.Models;
using Client.Models.AuthenModel;

namespace Client.Repositories.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        public Task<ResponseModel> LoginAsync(LoginRequestModel loginModel);
        public Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginModel);
        public Task<ResponseModel> RegisterAsync(RegisterModel user);
        public Task<ResponseModel> ChangePasswordAsync(ChangePasswordModel model);
        public Task<ResponseModel> ResetPasswordAsync(ResetPasswordModel model);
        public Task<ResponseModel> ForgotPasswordAsync(ForgotPasswordModel model);
        public Task<ResponseModel> ActiveUserAsync(string email);
        public Task<ResponseModel> LoginWithoutPassword(string email);
        public Task<ResponseModel> LogoutAsync();

    }
}
