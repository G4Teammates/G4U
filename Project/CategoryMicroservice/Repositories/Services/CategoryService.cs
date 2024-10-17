using AutoMapper;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.Models;
using CategoryMicroservice.Repositories.Interfaces;

namespace CategoryMicroservice.Repositories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly CategoryDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHelperService _helper;
        public Task<ResponseModel> AddCategory(CategoryModel category)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> DeleteCategory(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> GetCategory(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel> UpdateCategory(CategoryModel user)
        {
            throw new NotImplementedException();
        }
    }
}
