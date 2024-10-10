﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.Models;
using UserMicroservice.Repositories.Interfaces;
using IAuthenService = UserMicroservice.Repositories.Interfaces.IAuthenticationService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IAuthenService authenservice, IUserService userService) : ControllerBase
    {
        private readonly IAuthenService _authenService = authenservice;
        private readonly IUserService _userService = userService;



        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                ResponseModel response = await _userService.GetAll();
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


        [HttpPost]
        public async Task<ActionResult> Add([FromBody] UserModel user)
        {
            try
            {
                ResponseModel response = await _userService.AddUserAsync(user, true);
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

        [HttpGet("search")]
        public async Task<ActionResult> FindUsers([FromQuery] string? query)
        {
            try
            {
                ResponseModel response = await _userService.FindUsers(query)!;
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


        [Authorize(Roles = "User")]
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody]RegisterRequestModel registerRequestModel)
        {
            try
            {
                ResponseModel response = await _authenService.RegisterAsync(registerRequestModel);
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody]LoginRequestModel loginRequestModel)
        {
            try
            {
                ResponseModel response = await _authenService.LoginAsync(loginRequestModel);
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

        //[Authorize(Roles = "Admin")]
        [HttpDelete("/delete/{id}")]
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
        [HttpPut("/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserModel updatedUserModel)
        {
            // Kiểm tra xem ID trong URL có khớp với ID trong đối tượng được cập nhật không
            if (id != updatedUserModel.Id)
            {
                return BadRequest("User ID mismatch.");
            }

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
