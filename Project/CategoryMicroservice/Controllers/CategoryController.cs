using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Repositories.Interfaces;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Models;
using X.PagedList.Extensions;

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
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                var Cates = await _categoryService.GetById(id);
                if (Cates.IsSuccess)
                {
                    _responseModel.Result = Cates.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Cates.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll( int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Cates = await _categoryService.GetAll(pageNumber,pageSize);
                if (Cates.IsSuccess)
                {
                    _responseModel.Result = Cates.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Cates.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }         
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryModel model)
        {
            try
            {
                var Cates = await _categoryService.CreateCategory(model);
                if(Cates.IsSuccess)
                {
                    _responseModel.Result = Cates.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Cates.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
        [HttpPut]

        public async Task<IActionResult> UpdateCategory([FromForm] CategoryModel model)
        {
            try
            {
                var Cates = await _categoryService.UpdateCategrori(model);

                if (Cates.IsSuccess)
                {
                    _responseModel.Result = Cates.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Cates.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }


        [HttpDelete("{id?}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategory(id);
                _responseModel.Result = result;
                return Ok(_responseModel.Result);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while deleting the Category: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpGet("search={searchString}")]
        public async Task<IActionResult> Search([FromRoute] string searchString, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Cate = await _categoryService.Search(searchString, pageNumber, pageSize);
                if (Cate.IsSuccess)
                {
                    _responseModel.Result = Cate.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Cate.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while creating the Categrori: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
