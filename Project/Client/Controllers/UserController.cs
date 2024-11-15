using Client.Models.AuthenModel;
using Client.Models.Enum;
using Client.Models.Enum.UserEnum;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.User;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using LoginRequestModel = Client.Models.AuthenModel.LoginRequestModel;
using ResponseModel = Client.Models.ResponseModel;
using IAuthenticationService = Client.Repositories.Interfaces.Authentication.IAuthenticationService;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.Categories;
using Client.Models.CategorisDTO;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Client.Repositories.Interfaces.Order;
using Client.Models.OrderModel;
using MongoDB.Bson;
using System.Collections.ObjectModel;

namespace Client.Controllers
{
    public class UserController(
        IAuthenticationService authenService, 
        IUserService userService, 
        ITokenProvider tokenProvider, 
        IHelperService helperService, 
        IRepoProduct repoProduct, 
        ICategoriesService categoriesService, 
        IOrderService orderService) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;
        public readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoriesService = categoriesService;
        public readonly IOrderService _orderService = orderService;

        ICollection<ProductModel> productsAtCart = new List<ProductModel>();
        CartModel cart = new();


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
                    LoginResponseModel user = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
                    if (user == null)
                    {
                        TempData["error"] = "Login failed";
                        return View();
                    }

                    _tokenProvider.SetToken(user!.Token);
                    HttpContext.Response.Cookies.Append("IsLogin", response.IsSuccess.ToString());

                    IEnumerable<Claim> claim = HttpContext.User.Claims;
                    UserClaimModel userClaim = new UserClaimModel
                    {
                        Id = user.Id!,
                        Username = user.Username!,
                        Email = user.Email!,
                        Role = user.Role!,
                        DisplayName = user.DisplayName!,
                        Avatar = user.Avatar!
                    };
                    await _helperService.UpdateClaim(userClaim, HttpContext);

                    TempData["success"] = "Login success";
                    if (userClaim.Role == "Admin")
                    {
                        return RedirectToAction("AdminDashboard", "Admin");
                    }
                    else if (userClaim.Role == "User")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                TempData["error"] = "Login fail, check your username(or email) and password";
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

                    IEnumerable<Claim> claim = HttpContext.User.Claims;
                    UserClaimModel userClaim = new UserClaimModel
                    {
                        Id = user.Id!,
                        Username = user.Username!,
                        Email = user.Email!,
                        Role = user.Role!,
                        DisplayName = user.DisplayName!,
                        Avatar = user.Avatar
                    };
                    await _helperService.UpdateClaim(userClaim, HttpContext);

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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _tokenProvider.ClearToken();
            HttpContext.Response.Cookies.Delete("IsLogin");
            HttpContext.Response.Cookies.Delete("g_csrf_token");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _authenService.ForgotPasswordAsync(model);
                if (response.IsSuccess)
                {
                    TempData["success"] = "Check your email";
                    return View();
                }
                TempData["success"] = "Forgot password is fail";
            }
            return View();
        }


        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            ViewData["UserId"] = userId;
            ViewData["Token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var response = await _authenService.ResetPasswordAsync(model);
            if (response.IsSuccess)
            {
                TempData["success"] = "Reset password is success";
                return RedirectToAction(nameof(Logout));
            }
            TempData["error"] = "Reset password is fail";
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
                    TempData["success"] = "Register is success";
                    return RedirectToAction("Index", "Home");
                }
                TempData["success"] = "Register is fail";
            }
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

                        // Lưu URL của avatar vào updateProductModel
                        updateUser.Avatar = imageUrl;
                    }
                }
                //else
                //{

                //}


                IEnumerable<Claim> claim = HttpContext.User.Claims;
                UserClaimModel user = new UserClaimModel
                {
                    Id = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!,
                    Username = updateUser.Username!,
                    Email = claim.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!,
                    Role = claim.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!,
                    DisplayName = updateUser.DisplayName!,
                    Avatar = updateUser.Avatar!
                };

                await _helperService.UpdateClaim(user, HttpContext);
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

            // Nếu ModelState không hợp lệ, trả về lại updateProductModel để hiển thị lỗi
            return View(updateUser);
        }


        public IActionResult EditProfile()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Cart(CartModel model)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cart(ProductViewModel product)
        {
            productsAtCart.Add(product.Prod);
            
            cart.Products = productsAtCart;
            cart.Order = new OrderModel
            {
                CustomerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value,
                TotalPrice = productsAtCart.Sum(x => x.Price),
                Items = productsAtCart.Select(x => new OrderItemModel
                {
                    ProductId = x.Id,
                    ProductName = x.Name,
                    Price = x.Price,
                    PublisherName = x.UserName,
                    Quantity = 1
                }).ToList()
            };
            return View(cart);
        }


        public IActionResult CartRemoveProduct(string productId)
        {
            var product = productsAtCart.FirstOrDefault(Id => Id.Id == productId);
            productsAtCart.Remove(product);
            cart.Order.Items = productsAtCart.Select(x => new OrderItemModel
            {
                ProductId = x.Id,
                ProductName = x.Name,
                Price = x.Price,
                PublisherName = x.UserName,
                Quantity = 1
            }).ToList();

            return RedirectToAction("Cart",cart);
        }

        [HttpGet]
        public IActionResult PasswordSecurity()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordSecurity(ChangePasswordModel model)
        {
            model.Id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            var response = await _authenService.ChangePasswordAsync(model);
            if (response.IsSuccess)
            {
                TempData["success"] = response.Message;
            }
            else
            {
                TempData["error"] = response.Message;
            }
            return View();
        }

        public async Task<IFormFile> DownloadFileAsIFormFile(string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(fileUrl);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    // Tạo một đối tượng IFormFile từ stream
                    var fileName = Path.GetFileName(fileUrl);
                    return new FormFile(stream, 0, stream.Length, null, fileName);
                }
                else
                {
                    throw new Exception("Failed to download file.");
                }
            }
        }

        [HttpPost]
        [RequestSizeLimit(60 * 1024 * 1024)] // 50MB
        [RequestFormLimits(MultipartBodyLengthLimit = 60 * 1024 * 1024)] // Đặt giới hạn cho form multipart
        public async Task<IActionResult> UpdateProduct(UpdateProductModel updateProductModel)
        {
            //if (!ModelState.IsValid)
            //{
            //    var errors = ModelState.Values.SelectMany(v => v.Errors)
            //                                  .Select(e => e.ErrorMessage);
            //    return BadRequest(new { Errors = errors });
            //}

            try
            {
                ResponseModel? responsee = await _productService.GetProductByIdAsync(updateProductModel.Id);
                if (responsee == null)
                {
                    throw new Exception("Không thấy game nào có ID vậy hết");
                }
                ProductModel? product = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(responsee.Result));
                
                if (updateProductModel.Links == null)
                {
                    updateProductModel.Links = product.Links.ToList();
                }

                if (updateProductModel.UserName == null)
                {
                    updateProductModel.UserName = product.UserName;
                }

                var numOfView = product.Interactions.NumberOfViews;
                var numOfLike = product.Interactions.NumberOfLikes;
                var numOfDisLike = product.Interactions.NumberOfDisLikes;
                // Tạo đối tượng ScanFileRequest
                var request = new ScanFileRequest
                {
                    gameFile = updateProductModel.gameFile
                };

                // Gọi service UpdateProduct từ phía Client
                var response = await _productService.UpdateProductAsync(
                    updateProductModel.Id, updateProductModel.Name, updateProductModel.Description, updateProductModel.Price, updateProductModel.Sold,
                   numOfView, numOfLike,numOfDisLike, updateProductModel.Discount,
                    updateProductModel.Links, updateProductModel.Categories, (int)updateProductModel.Platform,
                    (int)updateProductModel.Status, updateProductModel.CreatedAt, updateProductModel.ImageFiles,
                    request, updateProductModel.UserName,updateProductModel.Interactions.UserLikes, updateProductModel.Interactions.UserDisLikes);

                if (response.IsSuccess)
                {
                    TempData["success"] = "Product updated successfully";
                    return RedirectToAction(nameof(EditProduct), new { id = updateProductModel.Id });
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return RedirectToAction(nameof(EditProduct), new { id = updateProductModel.Id });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string id)
        {
            try
            {
                ResponseModel? responsee = await _productService.GetProductByIdAsync(id);
                if (responsee == null)
                {
                    throw new Exception("Không thấy game nào có ID vậy hết");
                }
                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(responsee.Result));

                UpdateProductModel updateProductModel = new UpdateProductModel();
                updateProductModel.Id = model.Id;
                updateProductModel.Name = model.Name;
                updateProductModel.Description = model.Description;
                updateProductModel.Price = model.Price;
                updateProductModel.Discount = model.Discount;
                updateProductModel.Categories = model.Categories.Select(x => x.CategoryName).ToList();
                updateProductModel.Platform = model.Platform;
                updateProductModel.Status = model.Status;

                if (model.Links != null)
                {
                    List<string> filesImage = new List<string>();
                    List<IFormFile> filesGame = new List<IFormFile>();
                    foreach (var link in model.Links)
                    {
                        if (link.ProviderName.Contains("Cloudinary"))
                        {
                            // Sử dụng trong CreateProductModel
                            var file = await DownloadFileAsIFormFile(link.Url);
                            updateProductModel.ImageFiles.Add(file);
                            filesImage.Add(link.Url);
                            ViewBag.ImageFiles = filesImage;
                        }
                        else if (link.ProviderName.Contains("Google Drive"))
                        {
                            var file = await DownloadFileAsIFormFile(link.Url);
                            updateProductModel.gameFile = file;
                            //ViewBag.GameFileName = file.FileName;
                            //ViewBag.GameFileSize = file.Length;
                            filesGame.Add(file);
                            ViewBag.Games = filesGame;
                        }
                    }
                }

                var responseCategory = await _categoriesService.GetAllCategoryAsync(1, 99);
                //updateProductModel.Categories = (List<string>)response.Result;
                ICollection<CategoriesModel>? haha = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(responseCategory.Result.ToString()!));
                List<string> kha = new List<string>();
                if (haha == null)
                {
                    TempData["error"] = "Category Không có dữ liệu";
                }
                else
                {
                    foreach (var hi in haha)
                    {
                        kha.Add(hi.Name);
                    }
                }

                ViewBag.Categories = kha;

                return View(nameof(UploadProduct), updateProductModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(UserDashboard));
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadProduct(UpdateProductModel updateProductModel)
        {
            try
            {
                // Lay du lieu category
                var response = await _categoriesService.GetAllCategoryAsync(1, 99);
                ICollection<CategoriesModel>? cate = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));
                List<string> listCate = new List<string>();
                if (cate == null)
                {
                    throw new Exception("Category Không có dữ liệu");
                }
                else
                {
                    foreach (var item in cate)
                    {
                        listCate.Add(item.Name);
                    }
                }

                ViewBag.Categories = listCate;

                updateProductModel.Categories.Add(listCate[0]);

                //if (updateProductModel != null)
                //{
                //    updateProductModel = updateProductModel;
                //}

                return View(updateProductModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [RequestSizeLimit(60 * 1024 * 1024)] // 50MB
        [RequestFormLimits(MultipartBodyLengthLimit = 60 * 1024 * 1024)] // Đặt giới hạn cho form multipart
        public async Task<IActionResult> UploadProductPost(UpdateProductModel updateProductModel)
        {
            try
            {
                updateProductModel.Id = ObjectId.GenerateNewId().ToString();

                IEnumerable<Claim> claim = HttpContext.User.Claims;
                UserClaimModel userClaim = new UserClaimModel
                {
                    Id = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!,
                    Username = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!,
                    Email = claim.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!,
                    Role = claim.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!,
                };

                updateProductModel.UserName = userClaim.Username;
                // Tạo đối tượng ScanFileRequest
                var request = new ScanFileRequest
                {
                    gameFile = updateProductModel.gameFile
                };

                // Gọi API CreateProductAsync
                var response = await _productService.CreateProductAsync(
                    updateProductModel.Name,
                    updateProductModel.Description,
                    updateProductModel.Price,
                    updateProductModel.Discount,
                    updateProductModel.Categories,
                    (int)updateProductModel.Platform,
                    (int)updateProductModel.Status,
                    updateProductModel.ImageFiles,
                    request,
                    updateProductModel.UserName);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(UserDashboard));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return RedirectToAction(nameof(UploadProduct), new {updateProductModel = updateProductModel});
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(UserDashboard));
            }
        }

        //public async Task<IActionResult> UserDashboard(string userName)
        //{
        //    ProductViewModel pro = new();
        //    try
        //    {
        //        ResponseModel? response = await _productService.GetAllProductsByUserName(userName);


        //        if (response != null && response.IsSuccess)
        //        {
        //            pro.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
        //        }
        //        else
        //        {
        //            TempData["error"] = response?.Message;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = ex.Message;
        //    }
        //    return View(pro);
        //}

        public async Task<IActionResult> UserDashboard()
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            string i = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            ResponseModel? ProResponese = await _productService.GetAllProductsByUserName(un);
            ResponseModel? WishListResponse = await _userService.GetAllProductsInWishList(i);
            /*ResponseModel? response2 = await _userService.GetUserAsync(i);*/

            if (ProResponese != null && ProResponese.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                //ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
                List<ProductModel>? ListProduct = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(ProResponese.Result));
                /*List<UsersDTO>? model1 = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response1.Result));*/
                List<WishlistModel>? Wishlist = JsonConvert.DeserializeObject<List<WishlistModel>>(Convert.ToString(WishListResponse.Result));

                if (ListProduct != null)
                {

                    productViewModel.Product = ListProduct ?? new List<ProductModel>();
                    /*productViewModel.User = model1 ?? new List<UsersDTO>();*/
                    productViewModel.Wishlist = Wishlist ?? new List<WishlistModel>();
                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                    productViewModel.userID = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
                }
            }
            else
            {
                TempData["error"] = ProResponese?.Message + WishListResponse.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return View(productViewModel);
        }

        public IActionResult DownloadProduct()
        {
            return View();
        }
    }
}
