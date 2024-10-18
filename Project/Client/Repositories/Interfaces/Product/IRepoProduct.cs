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
        /*Task<ResponseModel> SearchProductAsync(string searchString);
        Task<ResponseModel> SortAsync(string sort);
        Task<ResponseModel> FilterAsync(decimal? minrange, decimal? maxrange, int? sold, bool? discount, int? platform, string? category);*/
    }
}

