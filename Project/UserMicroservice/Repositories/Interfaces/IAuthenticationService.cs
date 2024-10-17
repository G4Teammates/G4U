using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel);
        Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginGoogleRequestModel);
        Task<ResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel);
        Task<ResponseModel> ForgotPasswordAsync();
    }
}
