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
using System.Security.Cryptography;

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
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789_0123456789_0123456789"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.GivenName, model.DisplayName),
                new Claim("Avatar", model.Avatar),
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


        public ResponseModel DecodeToken(string token)
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

        public User GetUserIdFromToken(JwtSecurityToken token)
        {
            return new User
            {
                Id = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value,
                Username = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value
            };
        }

        public string GenerateRandomPassword(int length)
        {
            const string asciiChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] password = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int randomIndex = randomBytes[i] % asciiChars.Length;
                    password[i] = asciiChars[randomIndex];
                }
            }

            return new string(password);
        }
    }
}
