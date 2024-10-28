using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.DTO;

namespace ProductMicroservice.Repostories
{
    public interface IRepoProduct
    {
        /*Products CreateProduct(CreateProductModel Product);*/
        Task<Products> GetById(string id);
        Task<Products> UpdateProduct(List<IFormFile>? imageFiles, UpdateProductModel Product, IFormFile? gameFiles);
        IEnumerable<Products> Products { get; }
        void DeleteProduct(string id);
        IEnumerable<Products> Sort(string sort);
        IEnumerable<Products> Search(string searchstring);
		IEnumerable<Products> Filter(decimal? minrange, decimal? maxrange, int? sold , bool? Discount, int? Platform, string Category);

		Task<Products> Moderate(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username);
        Task<List<Products>> GetProductsByCategoryNameAsync(string categoryName);
    }
}
