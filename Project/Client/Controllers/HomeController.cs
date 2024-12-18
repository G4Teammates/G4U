﻿using Client.Models;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Product;

using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace Client.Controllers
{
    public class HomeController(ILogger<HomeController> logger, ITokenProvider tokenProvider, IHelperService helperService, IRepoProduct repoProduct) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ITokenProvider _tokenProvider = tokenProvider;
        private readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        private string JWT = "JWT";
        private string IsLogin = "IsLogin";
        private string RememberMe = "RememberMe";

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
        public async Task<IActionResult> Index(int? page, int pageSize = 99)
        {
            int pageNumber = page ?? 1;
            AllModel product = new();

            try
            {
                #region Check IsLogin Cookie and Token
                // Lấy token và kiểm tra tính hợp lệ, nhưng luôn tiếp tục để lấy sản phẩm
                bool isLogin = false;
                bool rememberMe = Convert.ToBoolean(_tokenProvider.GetToken(RememberMe));
                string token;

                if (rememberMe)
                {
                    token = _tokenProvider.GetToken(JWT);
                    isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin));
                }
                else
                {
                    token = _tokenProvider.GetToken(JWT, false);
                    isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin,false));
                }

                if (token == null)
                {
                    ViewData["IsLogin"] = false;
                    await HttpContext.SignOutAsync();
                }
                var response = _helperService.CheckAndReadToken(token);
                if (response.IsSuccess)
                {
                    var user = _helperService.GetUserFromJwtToken((JwtSecurityToken)response.Result);
                    ViewBag.User = user;
                    ViewData["IsLogin"] = true;
                    TempData["success"] = "Welcome back "+ user.Username;
                }
                else
                {
                    ViewData["IsLogin"] = false;
                }
                #endregion

                #region Lấy dữ liệu sản phẩm
                // Gọi API để lấy danh sách sản phẩm dựa trên phân trang
                var responseModel = await _productService.GetAllProductAsync(pageNumber, pageSize);
                // Gọi API một lần nữa để lấy tổng số sản phẩm (không phân trang)
                var totalProductsResponse = await _productService.GetAllProductAsync(1, 99);

                if (responseModel != null && responseModel.IsSuccess)
                {
                    // Đọc và gán dữ liệu sản phẩm cho model
                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(responseModel.Result.ToString()!);
                    var totalProducts = JsonConvert.DeserializeObject<ICollection<ProductModel>>(totalProductsResponse.Result.ToString()!);

                    product.pageNumber = pageNumber;
                    product.totalItem = totalProducts.Count;
                    product.pageSize = pageSize;
                    product.pageCount = (int)Math.Ceiling(totalProducts.Count / (double)pageSize);
                }
                else
                {
                    TempData["error"] = responseModel?.Message;
                }
                #endregion
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(product);
        }



        public IActionResult Play()
        {
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