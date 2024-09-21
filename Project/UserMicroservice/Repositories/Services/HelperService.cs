using Microsoft.EntityFrameworkCore;
using UserMicroservice.DBContexts;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.Services
{
    public class HelperService : IHelperService
    {
        private readonly UserDbContext _context;
        public HelperService(UserDbContext context)
        {
            _context = context;
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
