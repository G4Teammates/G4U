using Client.Models;
using Client.Models.ProductDTO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetAllProductAsync();
        Task<ResponseModel?> SearchProductAsync(string searchString);
        Task<ResponseModel> SortProductAsync(string sort);
        Task<ResponseModel> FilterProductAsync(decimal? minrange, decimal? maxrange, int? sold, bool? Discount, int? Platform, string Category);
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

        Task<ResponseModel> CreateProductAsync(string name,
            string description,
            decimal price,
            float discount,
            List<string> categories,
            int platform,
            int status,
            List<IFormFile> imageFiles,
            ScanFileRequest request,
            string username);

    }
}
