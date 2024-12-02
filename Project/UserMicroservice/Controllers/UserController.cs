    using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;
using IAuthenService = UserMicroservice.Repositories.Interfaces.IAuthenticationService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.DBContexts.Enum;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IAuthenService authenservice, IUserService userService) : ControllerBase
    {
        private readonly IAuthenService _authenService = authenservice;
        private readonly IUserService _userService = userService;

        private ResponseModel _responseModel = new ResponseModel();

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        [HttpGet]
        public async Task<ActionResult> GetAll(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                ResponseModel response = await _userService.GetAll(pageNumber, pageSize);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] AddUserModel user)
        {
            try
            {
                ResponseModel response = await _userService.AddUserAsync(user);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("search")]
        public async Task<ActionResult> FindUsers([FromQuery] string? query, int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                ResponseModel response = await _userService.FindUsers(query, pageNumber, pageSize)!;
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }


        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            try
            {
                ResponseModel response = await _userService.GetUser(id);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                ResponseModel response = await _userService.DeleteUser(id);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

        // Phương thức cập nhật người dùng
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpPut]
        public async Task<ActionResult> UpdateUser([FromBody] UserUpdate updatedUserModel)
        {
            try
            {
                ResponseModel response = await _userService.UpdateUser(updatedUserModel);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody]UserStatus status)
        {
            try
            {
                ResponseModel response = await _userService.ChangeStatus(id, status);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpGet("getAllProductsInWishList/{id}")]
        public async Task<ActionResult> GetAllProductsInWishList(string id)
        {
            try
            {
                var response = await _userService.GetAllProductsInWishList(id);
                if (response.IsSuccess)
                {
                    _responseModel.Result = response.Result;
                    return Ok(response);
                }
                _responseModel.IsSuccess= false;
                _responseModel.Message = response.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpPut("addWishList/{userName}")]
        public async Task<ActionResult> AddToWishList(UserWishlistModel userWishlistModel, [FromRoute] string userName)
        {
            try
            {
                var response = await _userService.AddToWishList(userWishlistModel, userName);
                if (response.IsSuccess)
                {
                    _responseModel.Result = response.Result;
                    return Ok(response);
                }
                _responseModel.IsSuccess = false;
                _responseModel.Message = response.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpPut("removeWishList/{userName}/{productId}")]
        public async Task<ActionResult> RemoveWishList([FromRoute] string productId, [FromRoute] string userName)
        {
            try
            {
                ResponseModel response = await _userService.RemoveFromWishList(productId, userName);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

        //[HttpGet("find")]
        //public async Task<ActionResult> SearchAsync([FromQuery]SearchCriteria query)
        //{
        //    var criteria = new SearchCriteriaBuilder()
        //     .SetDisplay(query.DisplayName)
        //     .SetStatus(query.Status)
        //     .SetEmail(query.Email)
        //     .SetPhoneNumber(query.PhoneNumber)
        //     .SetUsername(query.Username)
        //     .Build();

        //    var users = await _userService.FindUsers(criteria);
        //    return Ok(users);
        //}

    }
}
