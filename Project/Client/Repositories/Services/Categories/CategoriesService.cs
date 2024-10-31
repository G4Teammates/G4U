using CategoryMicroservice.DBContexts.Entities;
using Client.Models;
using Client.Models.CategorisDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Utility;
using Microsoft.CodeAnalysis;

namespace Client.Repositories.Services.Categories
{
    public class CategoriesService : ICategoriesService
    {
        public readonly IBaseService _baseService;
        public CategoriesService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseModel> CreateCategoryAsync(CreateCategories category)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(category.Description), "Description");
            formData.Add(new StringContent(category.Type.ToString()), "Type");
            formData.Add(new StringContent(category.Status.ToString()), "Status");
            formData.Add(new StringContent(category.Name), "Name");
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Category"
            });
        }

        public async Task<ResponseModel> DeleteCategoryAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Category/" + id
            });
        }

		public async Task<ResponseModel> GetAllCategoryAsync(int pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Category?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> GetCategoryAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Category/" + id
            });
        }

		public async Task<ResponseModel?> SearchProductAsync(string searchString, int? pageNumber, int pageSize)
		{
			return await _baseService.SendAsync(new RequestModel()
			{
				ApiType = StaticTypeApi.ApiType.GET,
				Url = $"{StaticTypeApi.APIGateWay}/Category/search={searchString}?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
			});
		}

		public async Task<ResponseModel> UpdateCategoryAsync(CategoriesModel category)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(category.Id), "Id");
            formData.Add(new StringContent(category.Description), "Description");
            formData.Add(new StringContent(category.Type.ToString()), "Type");
            formData.Add(new StringContent(category.Status.ToString()), "Status");
            formData.Add(new StringContent(category.Name), "Name");
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Category"
            });
        }
    }
}
