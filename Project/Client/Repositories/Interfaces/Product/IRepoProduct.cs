using Client.Models;
using Client.Models.ProductDTO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using CategoryModel = Client.Models.ProductDTO.CategoryModel;
using LinkModel = Client.Models.ProductDTO.LinkModel;

namespace Client.Repositories.Interfaces.Product
{
    public interface IRepoProduct
    {
        Task<ResponseModel?> GetProductByIdAsync(string Id);
        Task<ResponseModel?> GetDetailByIdAsync(string Id);
        Task<ResponseModel?> SearchProductAsync(string searchString, int? pageNumber, int pageSize);
        Task<ResponseModel> SortProductAsync(string sort, int? pageNumber, int pageSize);
        Task<ResponseModel> FilterProductAsync(decimal? minrange, decimal? maxrange, int? sold, bool? Discount, int? Platform, string Category, int? pageNumber, int pageSize);
        Task<ResponseModel> GetAllProductsByUserName(string userName);
        Task<ResponseModel?> GetAllProductAsync(int? pageNumber, int pageSize);

        Task<ResponseModel> UpdateProductAsync(string id,
                                                     string name,
                                                     string description,
                                                     decimal price,
                                                     int sold,
                                                     int numOfView,
                                                     int numOfLike,
                                                     float discount,
                                                     List<LinkModel> links,
                                                     List<string> categories,
                                                     int platform,
                                                     int status,
                                                     DateTime createdAt,
                                                     List<IFormFile> imageFiles,
                                                     ScanFileRequest? request,
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
        Task<ResponseModel> DeleteProductAsync(string Id);

        string GenerateQRCode(string productId);

        /*string GenerateBarCode(long barCodeUrl);*/
    }
}
