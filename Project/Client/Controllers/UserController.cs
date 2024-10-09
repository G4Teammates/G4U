using Client.Models.AuthenModel;
using Client.Repositories.Interfaces.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;

        public IActionResult Index()
        {
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
