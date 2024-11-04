using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CommentMicroservice.DBContexts;
using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models.DTO;
using CommentMicroservice.Repositories;
using CommentMicroservice.Models;
using X.PagedList.Extensions;

namespace CommentMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly IRepoComment _db;
        private ResponseModel _responseModel;
        public CommentController(IRepoComment context)
        {
            _db = context;
            _responseModel = new ResponseModel();
        }


        [HttpGet("{id?}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                var Cates = await _db.GetById(id);
                _responseModel.Result = Cates;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpGet("List/{id?}")]
        public async Task<IActionResult> GetListById([FromRoute] string id, int? page,int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Cates = await _db.GetListById(id);
                _responseModel.Result = Cates.ToPagedList(pageNumber,pageSize);
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpGet]
        public IActionResult GetAll(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = _db.Comments;
                _responseModel.Result = Comm.ToPagedList(pageNumber,pageSize);
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpPost]
        public IActionResult CreateComment([FromForm] CreateCommentDTO model)
        {
            try
            {
                var Comm = _db.CreateComment(model);
                _responseModel.Result = Comm;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateComment([FromForm] CommentModel model)
        {
            try
            {
                var Comm = await _db.UpdateComment(model);
                _responseModel.Result = Comm;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }


        [HttpDelete("{id?}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string id)
        {
            try
            {
                _db.DeleteComment(id);
                _responseModel.Result = null;
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpGet("search={searchString}")]
        public IActionResult Search([FromRoute] string searchString, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var SanPhams = _db.Search(searchString);
                _responseModel.Result = SanPhams.ToPagedList(pageNumber, pageSize);
                return Ok(_responseModel);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while creating the Categrori: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpGet("get={productId}")]
        public async Task<IActionResult> GetByproductId([FromRoute] string productId, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = await _db.GetByproductId(productId, pageNumber, pageSize);
                if (Comm.IsSuccess)
                {
                    _responseModel.Result = Comm.Result;
                    return Ok(_responseModel);
                }
                _responseModel.Message = Comm.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while getting the Comment: " + ex.Message;
                return StatusCode(500, _responseModel); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
