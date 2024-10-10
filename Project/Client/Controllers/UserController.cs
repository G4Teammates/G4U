using Client.Models.AuthenModel;
using Client.Repositories.Interfaces.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserMicroservice.Models;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public static string? login;
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Login"] = login;
            return View(login);
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var response = await _authenService.LoginAsync(loginModel);

                if (response.IsSuccess)
                {
                    login = loginModel.UsernameOrEmail;
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
