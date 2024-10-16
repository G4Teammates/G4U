using Client.Models;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
    }
}
