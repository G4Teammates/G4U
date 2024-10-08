using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserMicroservice.DBContexts;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.Models;

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
        }
        public Task<ResponseModel> ForgotPasswordAsync()
        {
            throw new NotImplementedException();
        }




        public async Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
        {
            var response = new ResponseModel();

            // Kiểm tra nếu `loginRequestModel` là null
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
                // Tìm người dùng dựa trên Username hoặc Email
                var user = await _context.Users.FirstOrDefaultAsync(x =>
                    x.NormalizedUsername == loginRequestModel.UsernameOrEmail ||
                    x.NormalizedEmail == loginRequestModel.UsernameOrEmail);

                // Kiểm tra nếu người dùng không tồn tại
                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or password is incorrect"
                    };
                }

                // Xác minh mật khẩu
                //bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginRequestModel.Password, user.PasswordHash);
                //if (!isPasswordCorrect)
                //{
                //    return new ResponseModel
                //    {
                //        IsSuccess = false,
                //        Message = "Username or password is incorrect"
                //    };
                //}

                // Tạo JWT Token
                string token = _helper.GenerateJwtAsync(user);

                // Chuẩn bị response thành công
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user.Username,
                    Id = user.Id,
                    Email = user.Email,
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


        public Task<ResponseModel> LoginWithGoggleAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> SignUpAsync()
        {
            throw new NotImplementedException();
        }
    }
}
