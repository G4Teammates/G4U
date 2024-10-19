using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Repositories.Interfaces;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Models;

namespace CategoryMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private ResponseModel _responseModel;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
            _responseModel = new ResponseModel();
        }

        [HttpGet("{id?}")]
        public IActionResult GetById([FromRoute] string id)
        {
            try
            {
                var Cates = _categoryService.GetById(id);
                _responseModel.Result = Cates;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var Cates = _categoryService.Categories;
                _responseModel.Result = Cates;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
        [HttpPost]
        public IActionResult CreateCategory([FromForm] CreateCategoryModel model)
        {
            try
            {
                var Cates = _categoryService.CreateCategory(model);
                _responseModel.Result = Cates;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
        [HttpPut]
        public IActionResult UpdateCategory([FromForm] CategoryModel model)
        {
            try
            {
                var Cates = _categoryService.UpdateCategrori(model);
                _responseModel.Result = Cates;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }


        [HttpDelete("{id?}")]
        public async Task<IActionResult> DeleteSanPham([FromRoute] string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategory(id);
                _responseModel.Result = result;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while deleting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
