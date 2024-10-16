using Client.Models;
using Client.Models.Product_Model;
using Client.Models.Product_Model.DTO;
using Client.Models.Product_Model.Entities;

namespace Client.Repositories.Interfaces.ProductInterface
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel?> CreateProductAsync(CreateProductModel createProduct);
        Task<ResponseModel?> UpdateProductAsync(UpdateProductModel product);
        Task<ResponseModel?> DeleteProductAsync(string id);
        Task<ResponseModel?> GetCategoriesAsync();

    }
}
