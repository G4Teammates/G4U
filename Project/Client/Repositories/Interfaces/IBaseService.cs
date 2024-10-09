using Client.Models;

namespace Client.Repositories.Interfaces
{
    public interface IBaseService
    {
        Task<ResponseModel> SendAsync(RequestModel requestDTO, bool withBearer = true);
    }
}
