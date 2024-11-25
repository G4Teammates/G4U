using Client.Models;
using static Client.Utility.StaticTypeApi;
using Client.Models.AuthenModel;
using Client.Repositories.Interfaces;
using IAuthenticationService = Client.Repositories.Interfaces.Authentication.IAuthenticationService;
using Client.Utility;
namespace Client.Repositories.Services.Authentication
{
    public class AuthenticationService(IBaseService baseService) : IAuthenticationService
    {
        private readonly IBaseService _baseService = baseService;
        private readonly string _authenUrl = APIGateWay + "/auth";


        public async Task<ResponseModel> ResetPasswordAsync(ResetPasswordModel model)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = model,
                Url = _authenUrl + "/reset-password"
            });
        }

        public async Task<ResponseModel> ChangePasswordAsync(ChangePasswordModel model)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = model,
                Url = _authenUrl + "/change-password"
            });
        }

        public async Task<ResponseModel> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = model,
                Url = _authenUrl + "/forgot-password"
            });
        }
        public async Task<ResponseModel> ActiveUserAsync(string email)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = email,
                Url = _authenUrl + "/active-user"
            });
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
        public async Task<ResponseModel> LoginWithoutPassword(string email)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = ApiType.POST,
                Data = email,
                Url = _authenUrl + "/login-without-password"
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

    }
}
