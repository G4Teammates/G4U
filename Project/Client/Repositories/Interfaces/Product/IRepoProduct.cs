using Client.Models;
using Client.Models.Product_Model;
using Client.Models.Product_Model.DTO;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel?> CreateProductAsync(CreateProductModel createProductModel);
        Task<ResponseModel?> DeleteProductAsync(string id);
    }
}
