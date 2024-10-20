using Client.Repositories.Interfaces;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Client.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Client.Models.AuthenModel;

namespace Client.Repositories.Services
{
	public class HelperService : IHelperService
	{
		private readonly Cloudinary _cloudinary = new Cloudinary(
			new Account(
			cloud: ConfigKeyModel.CloudinaryName,
			apiKey: ConfigKeyModel.CloudinaryKey,
			apiSecret: ConfigKeyModel.CloudinarySecret));


		public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
		{
			ImageUploadResult result = new();
			try
			{
				var UploadParam = new ImageUploadParams
				{
					File = new FileDescription(fileName, imageStream),
					Overwrite = true
				};
				result = await _cloudinary.UploadAsync(UploadParam);
				return result?.SecureUrl.AbsoluteUri ?? "Update fail, please try again";
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
        }
        public LoginResponseModel GetUserFromJwtToken(JwtSecurityToken token)
        {
            return new LoginResponseModel
            {
                Id = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                Username = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value,
                Email = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value,
                Avatar = token?.Claims.FirstOrDefault(claim => claim.Type == "Avatar")?.Value,
                DisplayName = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value,
                Role = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value,
                Token = token.ToString()
            };
        }

        public ResponseModel CheckAndReadToken(string token)
        {
            // Khởi tạo đối tượng ResponseModel mặc định
            if (string.IsNullOrEmpty(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Token is null"
                };
            }

            var handler = new JwtSecurityTokenHandler();

            // Kiểm tra nếu token không hợp lệ hoặc không thể đọc
            if (!handler.CanReadToken(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Invalid token format"
                };
            }

            // Đọc token
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jsonToken == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Unable to analyze token"
                };
            }

            // Trường hợp token hợp lệ
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Token is valid",
                Result = jsonToken
            };
        }

    }
}
