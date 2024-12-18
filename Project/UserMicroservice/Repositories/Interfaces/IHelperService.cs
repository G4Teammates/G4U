﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IHelperService
    {
        public Task<ResponseModel> IsUserNotExist(string username, string? email = null, string? phone = null);
        public ResponseModel IsUserNotNull(AddUserModel user);
        public ResponseModel NomalizeQuery(string? query);
        public string GenerateJwtAsync(UserModel user);
        public Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage);
        public string GetAppBaseUrl();
        public string GeneratePasswordResetToken(UserModel model);
        public ResponseModel DecodeToken(string token);
        public User GetUserIdFromToken(JwtSecurityToken token);
        public string GenerateRandomPassword(int length);
        public bool IsContentAppropriate(string content);
    }
}
