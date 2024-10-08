using UserMicroservice.Models;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel);
        Task<ResponseModel> LoginWithGoggleAsync();
        Task<ResponseModel> SignUpAsync();
        Task<ResponseModel> LogoutAsync();
        Task<ResponseModel> ForgotPasswordAsync();
    }
}
