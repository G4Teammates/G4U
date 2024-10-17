using Client.Models.AuthenModel;
using Client.Models.Enum;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using LoginRequestModel = Client.Models.AuthenModel.LoginRequestModel;

namespace Client.Controllers
{
    public class UserController(IAuthenticationService authenService, IUserService userService, ITokenProvider tokenProvider) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;



        [HttpGet]
        public IActionResult Login()
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
                    var user = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
                    _tokenProvider.SetToken(user!.Token);
                    HttpContext.Response.Cookies.Append("Login", user.Username);
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
                    EmailConfirmation = (User.EmailStatus)(payload.EmailVerified ? 1 : 0),
                    Picture = payload.Picture
                };
                var response = await _authenService.LoginGoogleAsync(loginGoogleRequestModel);
                if (response.IsSuccess)
                {
                    HttpContext.Response.Cookies.Append("Login", loginGoogleRequestModel.DisplayName);
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

        [HttpPost]
        public IActionResult Logout()
        {
            _tokenProvider.ClearToken();
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

        public IActionResult Information()
        {
            var token = _tokenProvider.GetToken();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var id = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

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

        public IActionResult Cart()
        {
            return View();
        }

    }
}
