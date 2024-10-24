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
			response.Message = "Username and email are not exist in database.";
			if (await _context.Users.AnyAsync(x =>
				x.Username == username ||
				x.Email == email ||
				x.PhoneNumber == phoneNumber &&
				phoneNumber!=null))
			{
				response.IsSuccess = false;
				response.Message = "Username or email already exist in database.";
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
