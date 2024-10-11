using Client.Models;
using Client.Models.ProductModel.DTO;
using Client.Models.ProductModel.Entities;

namespace Client.Repositories.Interfaces.ProductInterface
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(int Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel?> CreateProductAsync(CreateProductModel createProductDTO);
        Task<ResponseModel?> UpdateProductAsync(UpdateProductModel productDTO);
        Task<ResponseModel?> DeleteProductAsync(int id);
    }
}
