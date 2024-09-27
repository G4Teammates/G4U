using UserMicroservice.Models;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IHelperService
    {
        public Task<ResponseModel> IsUserNotExist(string username, string email);
        public ResponseModel IsUserNotNull(UserModel user);
        public ResponseModel NomalizeQuery(string? query);
    }
}
