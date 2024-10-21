using Client.Models;
using Client.Models.ProductDTO;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel> UpdateProductAsync(List<IFormFile> imageFiles,UpdateProductModel product,ScanFileRequest request);
        Task<ResponseModel> DeleteProductAsysnc(string Id);
    }
}

