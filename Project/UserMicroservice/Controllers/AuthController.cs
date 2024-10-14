using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;

namespace UserMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthenticationService authService) : ControllerBase
    {
        private readonly IAuthenticationService _authService = authService;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestModel loginRequestModel)
        {
            try
            {
                ResponseModel response = await _authService.LoginAsync(loginRequestModel);
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
        public async Task<ActionResult> Register([FromBody] RegisterRequestModel registerRequestModel)
        {
            try
            {
                ResponseModel response = await _authService.RegisterAsync(registerRequestModel);
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
    }
}
