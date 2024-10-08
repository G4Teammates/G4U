using AutoMapper;
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
    public class UserController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IAuthenService _authenService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public UserController(UserDbContext context, IMapper mapper, IUserService userService, IAuthenService authenService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _authenService = authenService;
        }

        [HttpPost("/create")]
        public async Task<ActionResult> Add([FromBody] UserModel user)
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

        [Authorize]
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


        [Authorize(Roles = "0,1,2,User,Admin")]
        [HttpGet("/{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            ResponseModel response = new();
            try
            {
                response = await _userService.GetUser(id);
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
        [HttpPost("/login")]
        public async Task<ActionResult> Login([FromBody]LoginRequestModel loginRequestModel)
        {
            ResponseModel response = new();
            try
            {
                response = await _authenService.LoginAsync(loginRequestModel);
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
