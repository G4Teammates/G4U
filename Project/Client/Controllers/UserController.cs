﻿using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserMicroservice.Models;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService, IUserService userService, ITokenProvider tokenProvider) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;




        [HttpGet]
        public async Task<IActionResult> UsersIndex()
        {
            List<UsersDTO?> list = new();
            ResponseModel? response = await _userService.GetAllUserAsync();

            if (response != null && response.IsSuccess)
            {

                list = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response.Result.ToString()));

            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(list);
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
