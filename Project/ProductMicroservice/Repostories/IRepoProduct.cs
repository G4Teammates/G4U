using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models;
using ProductMicroservice.Models.DTO;
using ProductMicroservice.Models.Message;

namespace ProductMicroservice.Repostories
{
    public interface IRepoProduct
    {
        /*Products CreateProduct(CreateProductModel Product);*/
        Task<ResponseDTO> GetById(string id);
        Task<ResponseDTO> GetDetail(string id);
        Task<ResponseDTO> UpdateProduct(List<IFormFile>? imageFiles, UpdateProductModel Product, IFormFile? gameFiles);
        Task<ResponseDTO> GetAll(int page, int pageSize);
        Task<ResponseDTO> DeleteProduct(string id);
        Task<ResponseDTO> Sort(string sort, int page, int pageSize);
        Task<ResponseDTO> Search(string searchstring, int page, int pageSize);
        Task<ResponseDTO> Filter(decimal? minrange, decimal? maxrange, int? sold , bool? Discount, int? Platform, string Category, int page, int pageSize);

        Task<ResponseDTO> Moderate(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username);
        Task<List<Products>> GetProductsByCategoryNameAsync(string categoryName);

        Task<ResponseDTO> TotalRequest();

        Task<ResponseDTO> GetAllProductsByUserName(string userName);

        // Phương thức tăng lượt thích cho bình luận
        Task<ResponseDTO> IncreaseLike(string productId,UserLikesModel userLike);

        // Phương thức giảm lượt thích cho bình luận
        Task<ResponseDTO> DecreaseLike(string productId, UserDisLikesModel userDisLike);
    }
}
