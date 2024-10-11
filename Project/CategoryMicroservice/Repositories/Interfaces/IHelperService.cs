using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Models;

namespace CategoryMicroservice.Repositories.Interfaces
{
    public interface IHelperService
    {
        public Task<ResponseModel> IsCategoryNotExist(string name);
        public ResponseModel IsCategoryNotNull(CategoryModel category);
        public ResponseModel NomalizeQuery(string? query);
    }
}
