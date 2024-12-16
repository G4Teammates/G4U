using Client.Repositories.Interfaces;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Client.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Client.Models.AuthenModel;
using Azure.AI.ContentSafety;
using Azure;
using Client.Models.Enum.ProductEnum;
using Client.Models.ProductDTO;
using System.IO;
using Client.Models.UserDTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using static Google.Apis.Requests.BatchRequest;
using Client.Utility;
using static Client.Models.Enum.UserEnum.User;
using Client.Repositories.Interfaces.Authentication;
using Microsoft.AspNetCore.Connections;

namespace Client.Repositories.Services
{
    public class HelperService(IBaseService baseService, ITokenProvider tokenProvider) : IHelperService
    {
        private readonly Cloudinary _cloudinary = new Cloudinary(
            new Account(
            cloud: ConfigKeyModel.CloudinaryName,
            apiKey: ConfigKeyModel.CloudinaryKey,
            apiSecret: ConfigKeyModel.CloudinarySecret));
        private IBaseService _baseService = baseService;
        private ITokenProvider _tokenProvider = tokenProvider;
        
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
                Token = token.ToString(),
                LoginType = token?.Claims.FirstOrDefault(claim => claim.Type == "LoginType")?.Value,
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

        public async Task<ResponseModel> Moderate(Stream imageFile)
        {
            ResponseModel responseResult = new ResponseModel();

            // Kiểm tra nếu stream rỗng hoặc không có dữ liệu
            if (imageFile == null || imageFile.Length == 0)
            {
                responseResult.IsSuccess = false;
                responseResult.Message = "No files provided.";
                return responseResult;
            }

            try
            {
                // Khởi tạo client của Azure Content Safety
                var client = new ContentSafetyClient(
                    new Uri(ConfigKeyModel.EndpointContentSafety!),
                    new AzureKeyCredential(ConfigKeyModel.ApiKeyContentSafety!)
                );

                // Đặt lại vị trí của stream về đầu
                imageFile.Position = 0;

                // Tạo đối tượng ContentSafetyImageData từ stream hiện tại
                var imageData = new ContentSafetyImageData(await BinaryData.FromStreamAsync(imageFile));
                var request = new AnalyzeImageOptions(imageData);

                Response<AnalyzeImageResult> response;

                try
                {
                    // Gửi yêu cầu kiểm duyệt hình ảnh tới Azure
                    response = await client.AnalyzeImageAsync(request);
                }
                catch (RequestFailedException ex)
                {
                    // Bắt lỗi từ Azure và trả về kết quả không thành công
                    responseResult.IsSuccess = false;
                    responseResult.Message = $"Analyze image failed. Status code: {ex.Status}, Error code: {ex.ErrorCode}, Error message: {ex.Message}";
                    return responseResult;
                }

                // Lấy kết quả kiểm duyệt từ phản hồi của Azure
                var result = response.Value;
                var hateSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity ?? 0;
                var selfHarmSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity ?? 0;
                var sexualSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity ?? 0;
                var violenceSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity ?? 0;

                // Kiểm tra mức độ nghiêm trọng của các danh mục (Hate, SelfHarm, Sexual, Violence)
                if (hateSeverity > 0 || selfHarmSeverity > 0 || sexualSeverity > 0 || violenceSeverity > 0)
                {
                    // Nếu hình ảnh có chứa nội dung nhạy cảm, trả về kết quả không thành công
                    responseResult.IsSuccess = false;
                    responseResult.Message = "Image is not safe";
                    return responseResult;
                }

                // Tạo model kiểm duyệt hình ảnh (CensorshipModel)
                var newCensor = new CensorshipModel()
                {
                    ProviderName = "Azure Content Safety",
                    Description = "Content Safety",
                    Status = CensorshipStatus.Access // Hoặc thay đổi giá trị phù hợp
                };

                // Trả về kết quả kiểm duyệt cùng với stream của ảnh
                responseResult.Result = new AvatarCensorshipModel
                {
                    AvatarFile = imageFile, // Trả về stream đã kiểm duyệt
                    Censorship = newCensor
                };

                responseResult.IsSuccess = true;
                responseResult.Message = "Image is safe";
                return responseResult; // Nếu hình ảnh an toàn, trả về thành công
            }
            catch (Exception ex)
            {
                // Bắt bất kỳ lỗi nào khác và trả về kết quả không thành công
                responseResult.IsSuccess = false;
                responseResult.Message = ex.Message;
                return responseResult;
            }
        }

        public async Task<HttpContext> UpdateClaim(UserClaimModel user, HttpContext context)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                bool isRememberMe = Convert.ToBoolean(_tokenProvider.GetToken("RememberMe"));
                if (authenticateResult.Succeeded)
                {
                    // Xóa claim cũ nếu tồn tại
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.GivenName, user.DisplayName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Avatar", user.Avatar),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("LoginType", user.LoginType)
                    };

                    // Đăng xuất người dùng
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Tạo ClaimsIdentity mới với claims mới
                    var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(newIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = isRememberMe ? DateTimeOffset.Now.AddDays(7) : DateTimeOffset.Now.AddDays(1)
                        }
                    );
                }
                else
                {
                    var claims = new List<Claim>
                    {
                           new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.GivenName, user.DisplayName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Avatar", user.Avatar),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("LoginType", user.LoginType)
                    };

                    var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(newIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = isRememberMe ? DateTimeOffset.Now.AddDays(7) : DateTimeOffset.Now.AddDays(1)
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                // Ném lỗi với thông tin chi tiết
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }

            return context;
        }


        public async Task<ResponseModel> SendMail(SendMailModel model)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = model,
                Url = StaticTypeApi.APIGateWay + "/Payment/send-notification"
            });
        }

    }
}