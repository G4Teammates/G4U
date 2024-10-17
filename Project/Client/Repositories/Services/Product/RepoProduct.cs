using Client.Models;
using Client.Models.Product_Model.DTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Product;
using Client.Utility;

namespace Client.Repositories.Services.Product
{
    public class RepoProduct : IRepoProduct
    {
        public readonly IBaseService _baseService;
        public RepoProduct(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public Task<ResponseModel?> CreateProductAsync(CreateProductModel createProductModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel?> DeleteProductAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel?> GetAllProductAsync()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product"
            });
        }

        public async Task<ResponseModel?> GetProductByIdAsync(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product/" + Id
            });
        }
    }
}
