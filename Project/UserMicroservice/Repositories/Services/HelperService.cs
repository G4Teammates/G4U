﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.Services
{
    public class HelperService : IHelperService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserDbContext _context;
        public HelperService(UserDbContext context, IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _jwtOptions = jwtOptions.Value;
        }

        //public string GenerateJwtAsync(User user)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();

        //    var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);

        //    var claimList = new List<Claim>
        //    {
        //        new Claim( JwtRegisteredClaimNames.Sub, user.Id),
        //        new Claim( JwtRegisteredClaimNames.Email, user.Email ),
        //        new Claim( JwtRegisteredClaimNames.UniqueName, user.Username),
        //        new Claim("role", user.Role.ToString())
        //    };


        //    var tokenDesriptor = new SecurityTokenDescriptor
        //    {
        //        Audience = _jwtOptions.Audience,
        //        Issuer = _jwtOptions.Issuer,
        //        Subject = new ClaimsIdentity(claimList),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDesriptor);
        //    return tokenHandler.WriteToken(token);
        //}
        public string GenerateJwtAsync(User account)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(ClaimTypes.Name, account.NormalizedUsername!),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ResponseModel> IsUserNotExist(string username, string email)
        {
            var response = new ResponseModel();
            response.Message = "Username and email are not exist in database. Ready to create new";
            if (await _context.Users.AnyAsync(x => x.Username == username || x.Email == email))
            {
                response.IsSuccess = false;
                response.Message = "Username or email already exist";
            }
            return response;
        }

        public ResponseModel IsUserNotNull(UserModel user)
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
