using AutoMapper;
using Azure;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.DBContexts.Enum;
using System.Security.Claims;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using Newtonsoft.Json;

namespace UserMicroservice.Repositories.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly UserDbContext _context;
        private readonly IHelperService _helper;
        private readonly IMapper _mapper;
        public AuthenticationService(
            UserDbContext context,
            IUserService userService,
            IHelperService helper,
            IMapper mapper
            )
        {
            _context = context;
            _userService = userService;
            _helper = helper;
            _mapper = mapper;
            _httpContextAccessor = new HttpContextAccessor();
        }
        private readonly IHttpContextAccessor _httpContextAccessor;



        public ResponseModel GetUserInfoByClaim(IEnumerable<Claim> claims)
        {
            ResponseModel response = new();
            try
            {
                var loginGoogleRequestModel = new LoginGoogleRequestModel
                {
                    Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    DisplayName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    EmailConfirmation = claims.ToString() == "true" ? EmailStatus.Confirmed : EmailStatus.Unconfirmed,
                    Picture = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                };
                response.Message = "Get user info by claim successful";
                response.Result = loginGoogleRequestModel;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Task<ResponseModel> GoogleCallback(LoginGoogleRequestModel loginGoogleRequestModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
        {
            loginRequestModel.UsernameOrEmail = loginRequestModel.UsernameOrEmail.ToUpper();
            var response = new ResponseModel();

            if (loginRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "LoginRequestModel is null"
                };
            }

            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.NormalizedUsername == loginRequestModel.UsernameOrEmail ||
                    x.NormalizedEmail == loginRequestModel.UsernameOrEmail);

                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or password is incorrect"
                    };
                }

                // Xác minh mật khẩu
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginRequestModel.Password, user.PasswordHash);
                if (!isPasswordCorrect)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or password is incorrect"
                    };
                }

                // Tạo JWT Token
                UserModel userModel = _mapper.Map<UserModel>(user);
                string token = _helper.GenerateJwtAsync(userModel);

                // Chuẩn bị response thành công
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Id = user.Id,
                    Email = user.Email,
                    Avatar = user.Avatar!,
                    Role = user.Role.ToString()
                };
                response.IsSuccess = true;
                response.Message = "Login successful";
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }




        public async Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginGoogleRequestModel)
        {
            var response = new ResponseModel();

            if (loginGoogleRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "RegisterRequestModel is null"
                };
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginGoogleRequestModel.Email);
                if (user == null)
                {
                    UserModel userCreateModel = new UserModel
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Email = loginGoogleRequestModel.Email!,
                        Username = loginGoogleRequestModel.Email!,
                        Role = UserRole.User,
                        Avatar = loginGoogleRequestModel.Picture!,
                        DisplayName = loginGoogleRequestModel.DisplayName
                    };
                    user = _mapper.Map<User>(userCreateModel);
                    await _context.AddAsync(user);
                    await _context.SaveChangesAsync();
                }

                UserModel userModel = _mapper.Map<UserModel>(user);
                string token = _helper.GenerateJwtAsync(userModel);
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user!.Username,
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
                response.Message = "Login successful";

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel)
        {
            var response = new ResponseModel();

            if (registerRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "RegisterRequestModel is null"
                };
            }

            try
            {
                // Kiểm tra xem username và email đã tồn tại chưa
                var isUserExist = await _helper.IsUserNotExist(registerRequestModel.Username, registerRequestModel.Email);
                if (!isUserExist.IsSuccess)
                {
                    return isUserExist;
                }
                UserModel userModel = _mapper.Map<UserModel>(registerRequestModel);
                userModel.Id = ObjectId.GenerateNewId().ToString();
                User userCreate = _mapper.Map<User>(userModel);
                userCreate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestModel.Password);
                await _context.AddAsync(userCreate);
                await _context.SaveChangesAsync();


                //Gửi mail kích hoạt tài khoản



                response.Result = userModel;

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> ForgotPasswordAsync(string email, string urlSuccess)
        {
            ResponseModel response = new();
            try
            {

                // Kiểm tra xem email có tồn tại không
                ResponseModel findUser = await _userService.GetUserByEmail(email);
                if (findUser == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Email does not exist";
                    return response;
                }
                UserModel user = (UserModel)findUser.Result;
                response = await SendPasswordResetEmailAsync(user);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        private async Task<ResponseModel> SendPasswordResetEmailAsync(UserModel model)
        {
            ResponseModel response = new();
            try
            {
                // Tạo token đặt lại mật khẩu
                var token = _helper.GeneratePasswordResetToken(model);

                // Tạo URL thủ công nếu không có ngữ cảnh HttpRequest
                string confirmationLink = $"{_helper.GetAppBaseUrl()}/User/ResetPassword?userId={model.Id}&token={token}";

                var emailSubject = "Đặt lại mật khẩu của bạn";
                var emailBody = $"Nhấp vào liên kết sau để đặt lại mật khẩu: <a href='{confirmationLink}'>Đặt lại mật khẩu</a>";

                // Kiểm tra và gửi email
                if (model.Email != null)
                {
                    response = await _helper.SendEmailAsync(model.Email, emailSubject, emailBody);
                    response.Result = token;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Email is null";
                }

            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.IsSuccess = false;
            }
            return response;
        }

        public async Task<ResponseModel> ResetPassword(string token, string newPassword)
        {
            ResponseModel response = new();
            try
            {
                if (token.IsNullOrEmpty())
                {
                    response.IsSuccess = false;
                    response.Message = "Token is null";
                    return response;
                }
                ResponseModel jsonToken = _helper.DecodeToken(token);
                User user = _helper.GetUserIdFromToken((JwtSecurityToken)jsonToken.Result);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.Users.Attach(user);     // Đính kèm vào context nhưng không theo dõi tất cả trường
                _context.Entry(user).Property(u => u.PasswordHash).IsModified = true; // Chỉ cập nhật trường cần thiết
                await _context.SaveChangesAsync();

                response.Message = "Reset password is success";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<ResponseModel> ChangePassword(string id,string oldPass, string newPassword)
        {
            ResponseModel response = new();
            try
            {
                User? user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User need change password is null";
                    return response;
                }
                else
                {
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(oldPass, user.PasswordHash);
                    if (isPasswordCorrect)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                        _context.Users.Attach(user);     // Đính kèm vào context nhưng không theo dõi tất cả trường
                        _context.Entry(user).Property(u => u.PasswordHash).IsModified = true; // Chỉ cập nhật trường cần thiết
                        await _context.SaveChangesAsync();
                        response.Message = "Change password is success";
                        return response;
                    }
                    response.IsSuccess = false;
                    response.Message = "Password is not correct";

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}