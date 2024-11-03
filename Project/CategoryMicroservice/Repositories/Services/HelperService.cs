using CategoryMicroservice.DBContexts;
using CategoryMicroservice.Models;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CategoryMicroservice.Repositories.Services
{
    public class HelperService : IHelperService
    {
        private readonly CategoryDbContext _context;
        public async Task<ResponseModel> IsCategoryNotExist(string name)
        {
            var response = new ResponseModel();
            response.Message = "Category are not exist in database. Ready to create new";
            response.Message = "Category are not exist in database. Ready to create new";
            if (await _context.Categories.AnyAsync(x => x.Name == name))
            {
                response.IsSuccess = false;
                response.Message = "Category already exist";
            }
            return response;
        }

        public ResponseModel IsCategoryNotNull(CategoryModel category)
        {
            var response = new ResponseModel();
            response.Message = "Category was not null";

            if (category == null)
            {
                response.IsSuccess = false;
                response.Message = "User is null";
            }
            return response;
        }

        public ResponseModel NomalizeQuery(string? query)
        {
            var response = new ResponseModel();
            if (string.IsNullOrEmpty(query))
            {
                response.IsSuccess = false;
                response.Message = "Query is null or empty";
            }
            else
            {
                response.Message = "Query is normalized";
                response.Result = query.ToUpper().Trim();
            }
            return response;
        }
    }
}
