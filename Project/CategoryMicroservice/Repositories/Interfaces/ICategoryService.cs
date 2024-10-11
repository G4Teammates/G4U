using CategoryMicroservice.Models;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface ICategoryService
    {
        Task<ResponseModel> GetAll();
        Task<ResponseModel> GetCategory(string id);
        Task<ResponseModel> AddCategory(CategoryModel category);
        Task<ResponseModel> UpdateCategory(CategoryModel category);
        Task<ResponseModel> DeleteCategory(string id);
        
    }
}
