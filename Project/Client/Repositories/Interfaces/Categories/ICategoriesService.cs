using Client.Models.CategorisDTO;

using Client.Models;

namespace Client.Repositories.Interfaces.Categories
{
    public interface ICategoriesService
    {
        Task<ResponseModel> GetAllCategoryAsync();

        Task<ResponseModel> GetCategoryAsync(string id);

        Task<ResponseModel> CreateCategoryAsync(CreateCategories category);

        Task<ResponseModel> UpdateCategoryAsync(UpdateCategories category);
    }
}
