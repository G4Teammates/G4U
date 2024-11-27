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

        /*IEnumerable<Category> Categories { get; }*/
        Task<ResponseModel> GetAll(int page, int pageSize);
        Task<ResponseModel> GetById(string id);
        Task<ResponseModel> CreateCategory(CreateCategoryModel Category);
        Task<ResponseModel> UpdateCategrori(CategoryModel Categrori);
        Task<string> DeleteCategory(string id);
        Task<ResponseModel> Search(string searchstring, int page, int pageSize);
        Task<bool> CheckCategorysByCategoryNameAsync(ICollection<CategoryNameModel> categories);
        Task<ResponseModel> GetCateByStatus(int Status);
        Task<ResponseModel> GetCateByType(int Type);
    }
}
