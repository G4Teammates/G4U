using Client.Models.AuthenModel;
using Client.Models.Enum;
using Client.Models.Enum.UserEnum;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Google.Apis.Auth;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using UserMicroservice.DBContexts.Entities;
using LoginRequestModel = Client.Models.AuthenModel.LoginRequestModel;
using ResponseModel = Client.Models.ResponseModel;

namespace Client.Controllers
{
	public class UserController(IAuthenticationService authenService, IUserService userService, ITokenProvider tokenProvider, IHelperService helperService) : Controller
	{
		private readonly IAuthenticationService _authenService = authenService;
		public readonly IUserService _userService = userService;
		public readonly ITokenProvider _tokenProvider = tokenProvider;
		public readonly IHelperService _helperService = helperService;
        


        [HttpGet]
		public IActionResult Login()
		{
			var isLogin = HttpContext.Request.Cookies["IsLogin"];
			ViewData["IsLogin"] = isLogin;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginRequestModel loginModel)
		{
			if (ModelState.IsValid)
			{
				var response = await _authenService.LoginAsync(loginModel);
				if (response.IsSuccess)
				{
					var user = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
					_tokenProvider.SetToken(user!.Token);
					HttpContext.Response.Cookies.Append("IsLogin", response.IsSuccess.ToString());
					return RedirectToAction("Index", "Home");
				}
				return RedirectToAction(nameof(Register), "User");
			}
			return View();

		}

		[Route("google-response")]
		public async Task<ActionResult> GoogleResponse()
		{
			var google_csrf_name = "g_csrf_token";
			try
			{
				var cookie = Request.Cookies[google_csrf_name];

				if (cookie == null)
				{
					return StatusCode((int)HttpStatusCode.BadRequest);
				}
				var requestbody = Request.Form[google_csrf_name];
				if (requestbody != cookie)
				{
					return StatusCode((int)HttpStatusCode.BadRequest);
				}
				var idtoken = Request.Form["credential"];
				GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idtoken).ConfigureAwait(false);
				LoginGoogleRequestModel loginGoogleRequestModel = new LoginGoogleRequestModel
				{
					Email = payload.Email,
					Username = payload.Email,
					DisplayName = payload.Name,
					EmailConfirmation = (Models.Enum.UserEnum.User.EmailStatus)(payload.EmailVerified ? 1 : 0),
					Picture = payload.Picture
				};
				var response = await _authenService.LoginGoogleAsync(loginGoogleRequestModel);
				if (response.IsSuccess)
				{
					var user = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
					_tokenProvider.SetToken(user.Token);
					HttpContext.Response.Cookies.Append("IsLogin", response.IsSuccess.ToString());
					return RedirectToAction("Index", "Home");
				}
				return RedirectToAction(nameof(Register), "User");
			}
			catch (Exception ex)
			{
				TempData["Error"] = ex.Message;
			}
			return RedirectToAction("Index");
		}

		public IActionResult Logout()
		{
			_tokenProvider.ClearToken();
			HttpContext.Response.Cookies.Delete("IsLogin");
			HttpContext.Response.Cookies.Delete("g_csrf_token");

			return RedirectToAction("Index", "Home");
		}

		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterModel register)
		{
			if (ModelState.IsValid)
			{
				var response = await _authenService.RegisterAsync(register);
				if (response.IsSuccess)
				{
					var user = JsonConvert.DeserializeObject<RegisterModel>(response.Result.ToString()!);
					//_tokenProvider.SetToken(user!.Token);

					return RedirectToAction("Index", "Home");
				}
			}
			return View();
		}

		public IActionResult ChangePassword()
		{
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Information()
		{
			var token = _tokenProvider.GetToken();
			var handler = new JwtSecurityTokenHandler();
			var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
			var id = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

			if (string.IsNullOrEmpty(id))
			{
				return RedirectToAction("Login", "User");
			}

			// Lấy thông tin người dùng từ dịch vụ
			var response = await _userService.GetUserAsync(id);
			if (response.IsSuccess)
			{
				var user = JsonConvert.DeserializeObject<UpdateUser>(response.Result.ToString()!);
				return View(user); // Truyền dữ liệu người dùng vào view
			}

			// Nếu không thành công, bạn có thể xử lý lỗi
			TempData["error"] = response.Message;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Information(UpdateUser updateUser)
		{
			if (ModelState.IsValid)
			{
                var token = _tokenProvider.GetToken();
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var username = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;


                // Gọi dịch vụ để cập nhật thông tin người dùng
                if (updateUser.AvatarFile != null && updateUser.AvatarFile.Length > 0)
                {
                    await using (var stream = updateUser.AvatarFile.OpenReadStream())
                    {
                        // Gọi phương thức kiểm duyệt từ _helperService
                        var resultModerate = await _helperService.Moderate(stream);
                        if (!resultModerate.IsSuccess)
                        {
                            TempData["error"] = "Avatar is not safe";
                            return RedirectToAction(nameof(Information));
                        }

                        // Đặt vị trí stream về đầu và upload lên Cloudinary
                        stream.Position = 0;
                        var imageUrl = await _helperService.UploadImageAsync(stream, updateUser.AvatarFile.FileName);

                        // Lưu URL của avatar vào model
                        updateUser.Avatar = imageUrl;
                    }
                }

                var response = await _userService.UpdateUser(updateUser);

				if (response.IsSuccess)
				{
					TempData["success"] = "User updated successfully";
					return RedirectToAction(nameof(Information));
				}
				else
				{
					TempData["error"] = response.Message;
				}
			}

			// Nếu ModelState không hợp lệ, trả về lại model để hiển thị lỗi
			return View(updateUser);
		}



		//[HttpPost]
		//public async Task<IActionResult> Information(ListUser user)
		//{
		//    if (ModelState.IsValid)
		//    {
		//        var updateUser = new ListUser
		//        {
		//            Username = user.Username,
		//            PhoneNumber = user.PhoneNumber,
		//            Email = user.Email,
		//            DisplayName = user.DisplayName
		//            // Nếu bạn có thêm thuộc tính, hãy thêm vào đây
		//        };

		//        ResponseModel? response = await _userService.UpdateUsers(updateUser);

		//        if (response != null && response.IsSuccess)
		//        {
		//            TempData["success"] = "User updated successfully";
		//            return RedirectToAction(nameof(Information));
		//        }
		//        else
		//        {
		//            TempData["error"] = response?.Message;
		//        }
		//    }

		//    // Nếu ModelState không hợp lệ, trả về lại model để hiển thị lỗi
		//    return View(user);

		//}


		public IActionResult EditProfile()
		{
			return View();
		}

		public IActionResult History()
		{
			return View();
		}

		public IActionResult Cart()
		{
			return View();
		}

	}
}
