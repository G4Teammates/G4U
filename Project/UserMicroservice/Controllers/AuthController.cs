using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Repositories.Interfaces;
using IAuthenticationService = UserMicroservice.Repositories.Interfaces.IAuthenticationService;
using System.Net;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json;
using UserMicroservice.DBContexts.Enum;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using System.Collections;
using Newtonsoft.Json.Linq;
namespace UserMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthenticationService authService, IHelperService helperService) : ControllerBase
    {
        private readonly IAuthenticationService _authService = authService;
        private readonly IHelperService _helperService = helperService;
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

        [AllowAnonymous]
        [HttpPost("login-google")]
        public async Task<ActionResult> LoginGoogle([FromBody] LoginGoogleRequestModel loginGoogleRequestModel)
        {
            try
            {
                ResponseModel response = await _authService.LoginGoogleAsync(loginGoogleRequestModel);
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
        [HttpPost("login-without-password")]
        public async Task<ActionResult> LoginWithoutPassword(string email)
        {
            try
            {
                ResponseModel response = await _authService.LoginWithoutPassword(email);
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


        /// Bắt đầu quá trình đăng nhập bằng Google.
        /// </summary>
        /// <returns>Chuyển hướng đến trang đăng nhập của Google.</returns>
        [HttpGet("signin-google")]
        [AllowAnonymous]
        public IActionResult ExternalLoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Xử lý callback sau khi đăng nhập bằng Google.
        /// </summary>
        /// <returns>Chuyển hướng về frontend với token JWT (nếu thành công đăng nhập thành công).</returns>
        [HttpGet("external-login-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            ResponseModel response = new();
            try
            {

                // Xác thực người dùng
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                if (!authenticateResult.Succeeded)
                {
                    response.IsSuccess = false;
                    response.Message = "Google authentication failed.";
                    return BadRequest(response);
                }
                ResponseModel userInfo = _authService.GetUserInfoByClaim(authenticateResult.Principal.Claims);
                if (!userInfo.IsSuccess)
                {
                    return BadRequest(response);
                }
                var user = await _authService.LoginGoogleAsync((LoginGoogleRequestModel)userInfo.Result);
                if (!user.IsSuccess)
                {
                    return BadRequest(response);
                }
                response.Result = user;
                response.Message = "Login successful";
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
            // Trả về thông tin người dùng
            return Ok(response);
        }

        [HttpPost("active-user")]
        [AllowAnonymous]
        public async Task<IActionResult> ActiveUser(string email)
        {
            try
            {
                ResponseModel response = await _authService.ActiveUserAsync(email);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            try
            {
                ResponseModel response = await _authService.ForgotPasswordAsync(forgotPasswordModel.Email);
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

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                ResponseModel response = await _authService.ResetPassword(model);
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

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            try
            {
                ResponseModel response = await _authService.ChangePassword(model);
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
