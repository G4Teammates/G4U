using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IHelperService
    {
        public Task<ResponseModel> IsUserNotExist(string username, string? email=null);
        public ResponseModel IsUserNotNull(UserModel user);
        public ResponseModel NomalizeQuery(string? query);
        public string GenerateJwtAsync(User user);
    }
}
