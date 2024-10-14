using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.DTO;

namespace ProductMicroservice.Repostories
{
    public interface IRepoProduct
    {
        /*Products CreateProduct(CreateProductModel Product);*/
        Products GetById(string id);
        Products UpdateProduct(UpdateProductModel Product);
        IEnumerable<Products> Products { get; }
        void DeleteProduct(string id);
        IEnumerable<Products> Sort(string sort);
        IEnumerable<Products> Search(string searchstring);
		IEnumerable<Products> Filter(decimal? minrange, decimal? maxrange, int? sold , bool? Discount, int? Platform, string Category);

		Task<Products> ModerateImages(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username);
    }
}
