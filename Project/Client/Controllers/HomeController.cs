using Client.Models;
using Client.Models.AuthenModel;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace Client.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ITokenProvider tokenProvider, IHelperService helperService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly IHelperService _helperService = helperService;


        public IActionResult Index()
        {
            try
            {
                #region Check IsLogin Cookie
                var isLogin = HttpContext.Request.Cookies["IsLogin"];
                if (string.IsNullOrEmpty(isLogin))
                {
                    // Trường hợp cookie không tồn tại
                    ViewData["IsLogin"] = false;
                }
                else
                {
                    ViewData["IsLogin"] = isLogin;
                }
                #endregion

                // Lấy token từ provider
                var token = _tokenProvider.GetToken();
                ResponseModel response = _helperService.CheckAndReadToken(token);
                if (!response.IsSuccess)
                {
                    ViewData["IsLogin"] = false;
                    return View();
                }
                LoginResponseModel user = _helperService.GetUserFromJwtToken((JwtSecurityToken)response.Result);

                ViewBag.User = user;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}