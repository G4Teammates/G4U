using System.Security.Claims;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel);
        Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginGoogleRequestModel);
        ResponseModel GetUserInfoByClaim(IEnumerable<Claim> claimsPrincipal);
        Task<ResponseModel> GoogleCallback(LoginGoogleRequestModel loginGoogleRequestModel);
        Task<ResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel);
        Task<ResponseModel> ForgotPasswordAsync(string email, string urlSuccess);
        Task<ResponseModel> ResetPassword(string token, string newPassword);
        Task<ResponseModel> ChangePassword(string id, string oldPassword, string newPassword);
    }
}
