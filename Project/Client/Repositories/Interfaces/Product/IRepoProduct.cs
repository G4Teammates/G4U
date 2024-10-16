using Client.Models;
using Client.Models.ProductDTO;


namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel> UpdateProductAsync(string id,
                                           string name,
                                           string description,
                                           decimal price,
                                           int sold,
                                           int numOfView,
                                           int numOfLike,
                                           float discount,
                                           List<string> categories,
                                           int platform,
                                           int status,
                                           DateTime createAt,
                                           List<IFormFile> imageFiles,
                                           ScanFileRequest request,
                                           string username);

    }
}
