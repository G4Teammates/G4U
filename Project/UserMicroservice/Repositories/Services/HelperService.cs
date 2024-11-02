using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Azure.Core;
using static System.Net.WebRequestMethods;
using System;
using Amazon.SecurityToken.Model;

namespace UserMicroservice.Repositories.Services
{
    public class HelperService(UserDbContext context, IHttpContextAccessor httpContextAccessor) : IHelperService
    {
        private readonly UserDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


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

        public async Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ResponseModel response = new();
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("kiet43012@gmail.com", "fjrq yuus fmaf ugbt"),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress("kiet43012@gmail.com"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                response.Message = "Email sent successfully";
                response.IsSuccess = true; 
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }

        public string GetAppBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : "http://defaultUrl";
        }



        public string GeneratePasswordResetToken(UserModel model)
        {
            // Tạo token bảo mật và mã hóa
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.GivenName, model.DisplayName),
                new Claim("Avatar", model.Avatar),
                new Claim("TokenType", "PasswordReset")
            };

            var token = new JwtSecurityToken(
                issuer: JwtOptionModel.Issuer,
                audience: JwtOptionModel.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signingCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }


    }
}
