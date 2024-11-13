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
                var Comm = await _db.GetById(id);
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

        [HttpGet("List/{id?}")]
        public async Task<IActionResult> GetListById([FromRoute] string id, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = await _db.GetListById(id, pageNumber, pageSize);
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

        [HttpGet]
        public async Task<IActionResult> GetAll(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = await _db.GetAll(pageNumber,pageSize);
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

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentDTO model)
        {
            try
            {
                var Comm = await _db.CreateComment(model);
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

        [HttpPut]
        public async Task<IActionResult> UpdateComment([FromForm] CommentModel model)
        {
            try
            {
                var Comm = await _db.UpdateComment(model);
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


        [HttpDelete("{id?}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string id)
        {
            try
            {
                var Comm = await _db.DeleteComment(id);
                if (Comm.IsSuccess)
                {
                    _responseModel.Result = Comm.Result;
                    _responseModel.Message = Comm.Message;
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

        [HttpGet("search={searchString}")]
        public async Task<IActionResult> Search([FromRoute] string searchString, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = await _db.Search(searchString,pageNumber, pageSize);
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

        [HttpGet("ParentId/{id?}")]
        public async Task<IActionResult> GetByParentId([FromRoute] string id, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Comm = await _db.GetByParentId(id, pageNumber, pageSize);
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

        [HttpPut("IncreaseLike/{commentId}")]
        public async Task<IActionResult> IncreaseLike([FromRoute] string commentId, [FromBody] UserLikesModel userLikes)
        {
            try
            {
                var result = await _db.IncreaseLike(commentId, userLikes);
                if (result.IsSuccess)
                {
                    _responseModel.Result = result.Result;
                    _responseModel.Message = result.Message;
                    return Ok(_responseModel);
                }
                _responseModel.Message = result.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while increasing the like count: " + ex.Message;
                return StatusCode(500, _responseModel);
            }
        }

        [HttpPut("DecreaseLike/{commentId}")]
        public async Task<IActionResult> DecreaseLike([FromRoute] string commentId, [FromBody] UserDisLikesModel userDisLikes)
        {
            try
            {
                var result = await _db.DecreaseLike(commentId, userDisLikes);
                if (result.IsSuccess)
                {
                    _responseModel.Result = result.Result;
                    _responseModel.Message = result.Message;
                    return Ok(_responseModel);
                }
                _responseModel.Message = result.Message;
                return BadRequest(_responseModel.Message);
            }
            catch (Exception ex)
            {
                _responseModel.IsSuccess = false;
                _responseModel.Message = "An error occurred while decreasing the like count: " + ex.Message;
                return StatusCode(500, _responseModel);
            }
        }

    }
}
