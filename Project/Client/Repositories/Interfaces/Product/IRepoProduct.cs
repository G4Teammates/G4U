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
                                                     int numOfDisLike,
                                                     float discount,
                                                     List<LinkModel> links,
                                                     List<string> categories,
                                                     int platform,
                                                     int status,
                                                     DateTime createdAt,
                                                     List<IFormFile> imageFiles,
                                                     ScanFileRequest? request,
                                                     string username,
                                                     List<string> userLikes,
                                                     List<string> userDisLike);

        Task<ResponseModel> UpdateProductCloneAsync(string id,
                                             string name,
                                             string description,
                                             decimal price,
                                             int sold,
                                             int numOfView,
                                             int numOfLike,
                                             int numOfDisLike,
                                             float discount,
                                             List<LinkModel> links,
                                             List<string> categories,
                                             int platform,
                                             int status,
                                             DateTime createdAt,
                                             List<IFormFile> imageFiles,
                                             ScanFileRequest? request,
                                             string username,
                                             List<string> userLikes,
                                             List<string> userDisLike,
                                             string? winrarPassword);

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

        Task<ResponseModel> CreateProductCloneAsync(string name,
            string description,
            decimal price,
            float discount,
            List<string> categories,
            int platform,
            int status,
            List<IFormFile> imageFiles,
            ScanFileRequest request,
            string username,
            string? winrarPassword);

        Task<ResponseModel> DeleteProductAsync(string Id);
        Task<ResponseModel> IncreaseLike(string productId, UserLikesModel userLikes);
        Task<ResponseModel> DecreaseLike(string productId, UserDisLikesModel userDisLikes);

        string GenerateQRCode(string productId);
        Task<ResponseModel> ViewMore(string viewString);
        /*string GenerateBarCode(long barCodeUrl);*/
    }
}
