﻿using AutoMapper;
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
using UserMicroService.DBContexts.Enum;

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
                var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginGoogleRequestModel.Email);
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
                    User userCreate = _mapper.Map<User>(userCreateModel);
                    await _context.AddAsync(userCreate);
                    await _context.SaveChangesAsync();
                }

                UserModel userModel = _mapper.Map<UserModel>(user);
                string token = _helper.GenerateJwtAsync(userModel);
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user!.Username,
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };


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
    }
}