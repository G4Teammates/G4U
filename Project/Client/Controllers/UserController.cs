using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserMicroservice.Models;
using LoginRequestModel = Client.Models.AuthenModel.LoginRequestModel;
using ResponseModel = Client.Models.ResponseModel;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService, IUserService userService, ITokenProvider tokenProvider) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;



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
                    HttpContext.Response.Cookies.Append("Login", user.Username);
					return RedirectToAction("Index", "Home");
                }
                return RedirectToAction(nameof(Register), "User");
            }
            return View();

        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }


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

        //public IActionResult Information()
        //{
        //    var token = _tokenProvider.GetToken();
        //    var handler = new JwtSecurityTokenHandler();
        //    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        //    var id = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

        //    return View();
        //}

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
                // Gọi dịch vụ để cập nhật thông tin người dùng
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

    }
}
