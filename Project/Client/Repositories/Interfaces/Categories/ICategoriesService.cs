using Client.Models.CategorisDTO;

using Client.Models;

namespace Client.Repositories.Interfaces.Categories
{
    public interface ICategoriesService
    {
        Task<ResponseModel> GetAllCategoryAsync(int pageNumber, int pageSize);

        Task<ResponseModel> GetCategoryAsync(string id);

        Task<ResponseModel> CreateCategoryAsync(CreateCategories category);

        Task<ResponseModel> UpdateCategoryAsync(CategoriesModel category);
        Task<ResponseModel> DeleteCategoryAsync(string id);

		Task<ResponseModel?> SearchProductAsync(string searchString, int? pageNumber, int pageSize);
	}
}
