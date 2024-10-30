using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.CategorisDTO;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Models.UserProductDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Services.AuthenticationService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Client.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ITokenProvider tokenProvider, IHelperService helperService, IRepoProduct repoProduct) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;


        //public IActionResult Index()
        //{
        //    try
        //    {
        //        #region Check IsLogin Cookie
        //        var isLogin = HttpContext.Request.Cookies["IsLogin"];
        //        if (string.IsNullOrEmpty(isLogin))
        //        {
        //            // Trường hợp cookie không tồn tại
        //            ViewData["IsLogin"] = false;
        //        }
        //        else
        //        {
        //            ViewData["IsLogin"] = isLogin;
        //        }
        //        #endregion

        //        // Lấy token từ provider
        //        var token = _tokenProvider.GetToken();
        //        ResponseModel response = _helperService.CheckAndReadToken(token);
        //        if (!response.IsSuccess)
        //        {
        //            ViewData["IsLogin"] = false;
        //            return View();
        //        }
        //        LoginResponseModel user = _helperService.GetUserFromJwtToken((JwtSecurityToken)response.Result);

        //        ViewBag.User = user;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //    }
        //    return View();
        //}
        public async Task<IActionResult> Index(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            AllModel product = new();
            try
            {

                // Lấy danh sách sản phẩm
                ResponseModel? productResponse = await _productService.GetAllProductAsync(pageNumber, pageSize);
                ResponseModel? allProductsResponse = await _productService.GetAllProductAsync(1, 99);

                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(allProductsResponse.Result.ToString()!));

                if (productResponse != null && productResponse.IsSuccess)
                {
                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(productResponse.Result.ToString()!));

                    var data = product.Product;
                    product.pageNumber = pageNumber;
                    product.totalItem = data.Count;
                    product.pageSize = pageSize;
                    product.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                }
                else
                {
                    TempData["error"] = productResponse?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(product);
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