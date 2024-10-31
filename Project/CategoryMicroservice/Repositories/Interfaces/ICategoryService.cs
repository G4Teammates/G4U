using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Models;
using CategoryMicroservice.Models.DTO;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface ICategoryService
    {
        /*Task<ResponseModel> GetAll();
        Task<ResponseModel> GetCategory(string id);
        Task<ResponseModel> AddCategory(CategoryModel category);
        Task<ResponseModel> UpdateCategory(CategoryModel category);
        Task<ResponseModel> DeleteCategory(string id);*/

        IEnumerable<Category> Categories { get; }
        Task<Category> GetById(string id);
        Category CreateCategory(CreateCategoryModel Category);
        Task<Category> UpdateCategrori(CategoryModel Categrori);
        Task<string> DeleteCategory(string id);
    }
}
