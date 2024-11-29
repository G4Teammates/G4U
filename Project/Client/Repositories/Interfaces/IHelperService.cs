using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.UserDTO;
using CloudinaryDotNet;
using System.IdentityModel.Tokens.Jwt;

namespace Client.Repositories.Interfaces
{
	public interface IHelperService
	{
		public Task<string> UploadImageAsync(Stream imageStream, string fileName);
        public LoginResponseModel GetUserFromJwtToken(JwtSecurityToken token);
		public ResponseModel CheckAndReadToken(string token);
		public Task<ResponseModel> Moderate(Stream imageFiles);
		public Task<HttpContext> UpdateClaim(UserClaimModel user, HttpContext context);
		public Task<ResponseModel> SendMail(SendMailModel model);
    }
}
