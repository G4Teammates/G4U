using Client.Models;
using Client.Models.ProductDTO;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel> UpdateProductAsync(
            UpdateProductModel product,
            List<IFormFile> imageFiles,
            ScanFileRequest request);
        Task<ResponseModel> DeleteProductAsysnc(string Id);
    }
}
