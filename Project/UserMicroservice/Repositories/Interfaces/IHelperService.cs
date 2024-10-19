using System.IdentityModel.Tokens.Jwt;
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
    }
}
