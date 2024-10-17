using Client.Models;
using static Client.Utility.StaticTypeApi;
using Client.Models.AuthenModel;
using Client.Repositories.Interfaces;
using IAuthenticationService = Client.Repositories.Interfaces.Authentication.IAuthenticationService;
using Client.Utility;
namespace Client.Repositories.Services.AuthenticationService
{
    public class AuthenticationService(IBaseService baseService) : IAuthenticationService
    {
        private readonly IBaseService _baseService = baseService;
        private readonly string _authenUrl = APIGateWay + "/auth";
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
                Url = _authenUrl + "/login"
            });
        }

        public async Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginModel)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = loginModel,
                Url = _authenUrl + "/login-google"
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
