using Client.Models;
using Client.Models.Product_Model.DTO;
using Client.Models.Product_Model.Entities;

namespace Client.Repositories.Interfaces.ProductInterface
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(int Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel?> CreateProductAsync(string name, string description, decimal price, float discount, List<string> categories, int platform, int status, List<IFormFile> imageFiles, ScanFileRequest request);
        Task<ResponseModel?> UpdateProductAsync(UpdateProductModel productDTO);
        Task<ResponseModel?> DeleteProductAsync(int id);

    }
}
