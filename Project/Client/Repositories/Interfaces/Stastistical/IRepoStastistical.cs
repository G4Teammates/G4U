using Client.Models;

namespace Client.Repositories.Interfaces.Stastistical
{
    public interface IRepoStastistical
    {
        Task<ResponseModel?> GetAll(int page, int pageSize);
    }
}
