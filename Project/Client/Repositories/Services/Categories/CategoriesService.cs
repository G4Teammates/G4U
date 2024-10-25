using CategoryMicroservice.DBContexts.Entities;
using Client.Models;
using Client.Models.CategorisDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Utility;

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
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = category,
                Url = StaticTypeApi.APIGateWay + "/Category"
            });
        }

        public async Task<ResponseModel> GetAllCategoryAsync()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Category"
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

        public async Task<ResponseModel> UpdateCategoryAsync(UpdateCategories category)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = category,
                Url = StaticTypeApi.APIGateWay + "/Category"
            });
        }
    }
}
