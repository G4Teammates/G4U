using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.DTO;

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
    }
}
