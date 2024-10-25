using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;

namespace UserMicroservice.Repositories.Services
{
    public class HelperService(UserDbContext context) : IHelperService
    {
        private readonly UserDbContext _context = context;

        public string GenerateJwtAsync(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptionModel.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Avatar", user.Avatar),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: JwtOptionModel.Issuer,
                audience: JwtOptionModel.Audience,
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public async Task<ResponseModel> IsUserNotExist(string username, string? email = null, string? phoneNumber = null)
        {
            var response = new ResponseModel();
            var count = new CountModel();

            try
            {  
                // Kiểm tra số lượng username trùng khớp
                if (!string.IsNullOrEmpty(username))
                {
                    count.NumUsername = await _context.Users.CountAsync(u => u.Username == username);
                }

                // Kiểm tra số lượng email trùng khớp
                if (!string.IsNullOrEmpty(email))
                {
                    count.NumEmail = await _context.Users.CountAsync(u => u.Email == email);
                }

                // Kiểm tra số lượng phoneNumber trùng khớp
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    count.NumPhoneNumber = await _context.Users.CountAsync(u => u.PhoneNumber == phoneNumber);
                }

                // Xây dựng thông báo phản hồi dựa trên kết quả
                var messages = new List<string>();

                if (count.NumUsername > 0)
                {
                    messages.Add($"Username '{username}' already exists in the database.");
                }

                if (count.NumEmail > 0)
                {
                    messages.Add($"Email '{email}' already exists in the database.");
                }

                if (count.NumPhoneNumber > 0)
                {
                    messages.Add($"Phone number '{phoneNumber}' already exists in the database.");
                }

                // Thiết lập thông báo dựa trên kết quả của từng field
                if (messages.Any())
                {
                    response.Message = string.Join(" ", messages); // Ghép các thông báo lại
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "Username, email, and phone number are all available.";
                    response.IsSuccess = true;
                }

                response.Result = count;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }


            return response;
        }


        public ResponseModel IsUserNotNull(AddUserModel user)
        {
            var response = new ResponseModel();
            response.Message = "User was not null";

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User is null";
            }
            return response;
        }

        public ResponseModel IsUserNotNull(User user)
        {
            var response = new ResponseModel();
            response.Message = "User was not null";

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User is null";
            }
            return response;
        }

        public ResponseModel NomalizeQuery(string? query)
        {
            var response = new ResponseModel();
            if (string.IsNullOrEmpty(query))
            {
                response.IsSuccess = false;
                response.Message = "Query is null or empty";
            }
            else
            {
                response.Message = "Query is normalized";
                response.Result = query.ToUpper().Trim();
            }
            return response;
        }
    }
}
