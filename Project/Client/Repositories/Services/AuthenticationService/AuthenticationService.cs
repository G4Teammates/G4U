using Client.Models;
using static Client.Utility.StaticTypeApi;
using Client.Models.AuthenModel;
using Client.Repositories.Interfaces;
using IAuthenticationService = Client.Repositories.Interfaces.Authentication.IAuthenticationService;
namespace Client.Repositories.Services.AuthenticationService
{
    public class AuthenticationService(IBaseService baseService) : IAuthenticationService
    {
        readonly IBaseService _baseService = baseService;
        readonly string _authenUrl = ApiUrl;
        public Task<ResponseModel> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> ForgotPasswordAsync(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> LoginAsync(LoginRequestModel loginModel)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = loginModel,
                Url = _authenUrl + "login"
            });
        }

        public Task<ResponseModel> LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> RegisterAsync(RegisterModel user)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = user,
                Url = _authenUrl + "/register"
            });
        }

        public Task<ResponseModel> ResetPasswordAsync(string username, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
