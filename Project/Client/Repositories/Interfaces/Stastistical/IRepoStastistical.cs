using Client.Models;
using Client.Models.Statistical;

namespace Client.Repositories.Interfaces.Stastistical
{
    public interface IRepoStastistical
    {
        Task<ResponseModel?> GetAll(int page, int pageSize);
        Task<ResponseModel?> GetByUser(TotalGroupByUserRequest totalGroupByUserRequest);
    }
}
