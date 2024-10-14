using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserMicroservice.Models;
using LoginRequestModel = Client.Models.AuthenModel.LoginRequestModel;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService, IUserService userService) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;




		[HttpPost]
        public async Task<IActionResult> Login(LoginRequestModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var response = await _authenService.LoginAsync(loginModel);
                if (response.IsSuccess)
                {
                    var user = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
                    CookieOptions options = new CookieOptions
                    {
                        HttpOnly = true, // Đảm bảo cookie chỉ có thể được truy cập thông qua HTTP (an toàn hơn)
                        Secure = true // Đảm bảo cookie chỉ truyền qua HTTPS
                    };
                    HttpContext.Response.Cookies.Append("Login", user!.Username, options);
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

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult Information()
        {
            return View();
        }


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
