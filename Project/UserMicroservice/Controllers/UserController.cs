﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.Models;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public UserController(UserDbContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody]UserModel user)
        {
            ResponseModel response = new();
            try
            {
                response = await _userService.AddUser(user);
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
        [HttpGet("/{id:guid}")]
        public ActionResult Get(Guid id)
        {
            var users = _context.Users.ToList();
            var user = users.Find(u => u.Id == id);
            return Ok(user);
        }

        [HttpGet("/search")]
        public async Task<ActionResult> FindUsers([FromQuery] string? query)
        {
            ResponseModel response = new();
            try
            {
                response = await _userService.FindUsers(query);
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
