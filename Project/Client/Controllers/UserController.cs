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
using AutoMapper;
using Azure;
using static Client.Models.Enum.UserEnum.User;
using Microsoft.AspNetCore.Http;

namespace Client.Controllers
{
    public class UserController(
        IAuthenticationService authenService,
        IUserService userService,
        ITokenProvider tokenProvider,
        IHelperService helperService,
        IRepoProduct repoProduct,
        ICategoriesService categoriesService,
        IOrderService orderService,
        IMapper mapper) : Controller
    {
        private readonly IAuthenticationService _authenService = authenService;
        public readonly IUserService _userService = userService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;
        public readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoriesService = categoriesService;
        public readonly IOrderService _orderService = orderService;
        public readonly IMapper _mapper = mapper;

        private string JWT = "JWT";
        private string IsLogin = "IsLogin";
        private string cart = "cart";
        private string RememberMe = "RememberMe";
        private string GoogleToken = "g_csrf_token";

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["IsLogin"] = _tokenProvider.GetToken(IsLogin);
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
                        TempData["error"] = response.Message;
                        return View();
                    }

                    if (user.IsRememberMe)
                    {
                        _tokenProvider.SetToken(JWT,user!.Token, 7);
                        _tokenProvider.SetToken(IsLogin, response.IsSuccess.ToString(), 7);
                        _tokenProvider.SetToken(RememberMe,user.IsRememberMe.ToString(), 7);
                    }
                    else
                    {
                        _tokenProvider.SetToken(JWT,user!.Token, 1,false);
                        _tokenProvider.SetToken(IsLogin, response.IsSuccess.ToString(), 1, false);
                        _tokenProvider.SetToken(RememberMe, user.IsRememberMe.ToString(), 1);

                    }
                    IEnumerable<Claim> claim = HttpContext.User.Claims;
                    UserClaimModel userClaim = new UserClaimModel
                    {
                        Id = user.Id!,
                        Username = user.Username!,
                        Email = user.Email!,
                        Role = user.Role!,
                        DisplayName = user.DisplayName!,
                        Avatar = user.Avatar!,
                        LoginType = user.LoginType,
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
                else
                {
                    TempData["error"] = response.Message;
                }
            }
            return View();

        }

        [Route("google-response")]
        public async Task<ActionResult> GoogleResponse()
        {
            try
            {
                var cookie = Request.Cookies[GoogleToken];

                if (cookie == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest);
                }
                var requestbody = Request.Form[GoogleToken];
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
                        Avatar = user.Avatar,
                        LoginType = user.LoginType
                    };
                    await _helperService.UpdateClaim(userClaim, HttpContext);

                    _tokenProvider.SetToken(JWT,user.Token, 7);
                    _tokenProvider.SetToken(IsLogin,response.IsSuccess.ToString(), 7);
                    _tokenProvider.SetToken(RememberMe,"true", 7);

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
            _tokenProvider.ClearToken(IsLogin);
            _tokenProvider.ClearToken(GoogleToken);
            _tokenProvider.ClearToken(RememberMe);
            _tokenProvider.ClearToken(JWT);
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
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                string email = claim.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;
                string username = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;

                var responseResetPassword = await _helperService.SendMail(new Models.SendMailModel()
                {
                    Email = email,
                    Subject = "Reset password was success",
                    Body = $"Account with {username} used Forgot password and Reset password success"
                });
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
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Invalid registration data. Please check your inputs.";
                    return View(register);
                }

                // Gọi service để thực hiện đăng ký
                var response = await _authenService.RegisterAsync(register);
                if (response.IsSuccess && response.Result != null)
                {
                    // Deserialize kết quả từ service
                    var user = JsonConvert.DeserializeObject<RegisterModel>(response.Result.ToString()!);
                    if (user != null)
                    {
                        TempData["success"] = "Registration successful. Check your email to activate your account.";
                        return View();
                    }
                    else
                    {
                        TempData["error"] = "Registration was successful, but the user data could not be processed.";
                    }
                }
                else
                {
                    // Xử lý lỗi nếu đăng ký thất bại
                    TempData["error"] = response.Message ?? "Registration failed. Please try again.";
                    return View(register);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi không mong muốn
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
            }
            // Trả về View với dữ liệu đã nhập nếu có lỗi
            return View(register);
        }

        [HttpGet]
        public async Task<IActionResult> ActiveUser(string? userId)
        {
            try
            {
                // Tìm kiếm user theo email
                ResponseModel findUser = await _userService.GetUserAsync(userId);
                if (!findUser.IsSuccess || findUser.Result == null)
                {
                    TempData["error"] = $"No user found with Id: {userId}";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin user
                UsersDTO user = JsonConvert.DeserializeObject<UsersDTO>(findUser.Result.ToString()!);

                // Thay đổi trạng thái của user
                UpdateUser updateUser = _mapper.Map<UpdateUser>(user);
                updateUser.Status = Models.Enum.UserEnum.User.UserStatus.Active;
                updateUser.EmailConfirmation = Models.Enum.UserEnum.User.EmailStatus.Confirmed;
                ResponseModel response = await _userService.UpdateUser(updateUser);
                response = await _authenService.LoginWithoutPassword(updateUser.Email);
                if (response.IsSuccess)
                {
                    LoginResponseModel userLogin = JsonConvert.DeserializeObject<LoginResponseModel>(response.Result.ToString()!);
                    if (user == null)
                    {
                        TempData["error"] = "Login failed";
                        return View();
                    }

                    _tokenProvider.SetToken(JWT,userLogin!.Token, 7);
                    _tokenProvider.SetToken(IsLogin,response.IsSuccess.ToString(), 7);
                    _tokenProvider.SetToken(RememberMe,"true", 7);

                    IEnumerable<Claim> claim = HttpContext.User.Claims;
                    UserClaimModel userClaim = new UserClaimModel
                    {
                        Id = userLogin.Id!,
                        Username = userLogin.Username!,
                        Email = userLogin.Email!,
                        Role = userLogin.Role!,
                        DisplayName = userLogin.DisplayName!,
                        Avatar = userLogin.Avatar!
                    };
                    await _helperService.UpdateClaim(userClaim, HttpContext);

                    TempData["success"] = "Login success";
                }
                if (response.IsSuccess)
                {
                    TempData["success"] = "User activation is successful.";
                    return View(); // Trả về View nếu cần hiển thị thông báo
                }
                else
                {
                    TempData["error"] = $"Failed to activate user. Reason: {response.Message}";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi bất ngờ
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }





        [HttpGet]
        public async Task<IActionResult> Information()
        {
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

            if (token == null || !isLogin)
            {
                TempData["error"] = "Session expired, please login again.";
                return View(nameof(Login));
            }
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

                var response = await _userService.UpdateUser(updateUser);


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
        public IActionResult Cart()
        {
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
                isLogin = Convert.ToBoolean(_tokenProvider.GetToken(IsLogin, false));
            }

            if (token == null || !isLogin)
            {
                TempData["error"] = "Session expired, please login again.";
                return View(nameof(Login));
            }





            // Đọc cookie giỏ hàng
            string cartJson = HttpContext.Request.Cookies["cart"];
            CartModel cart = new()
            {
                Order = new OrderModel()
            };
            if (!string.IsNullOrEmpty(cartJson))
            {
                // Chuyển đổi JSON thành đối tượng CartViewModel
                cart = JsonConvert.DeserializeObject<CartModel>(cartJson);

                return View(cart);
            }

            return View(cart);
            // Truyền dữ liệu vào View
        }


        [HttpPost]
        public IActionResult Cart(ProductViewModel product)
        {
            // Đọc cookie giỏ hàng
            string cartJsonCookie = HttpContext.Request.Cookies["cart"];
            CartModel cart = new();

            if (!string.IsNullOrEmpty(cartJsonCookie))
            {
                // Chuyển đổi JSON thành đối tượng CartModel
                cart = JsonConvert.DeserializeObject<CartModel>(cartJsonCookie);
            }
            // Nếu giỏ hàng chưa được khởi tạo thì khởi tạo mới
            if (cart.Order == null)
            {
                cart.Order = new OrderModel
                {
                    Items = new List<OrderItemModel>(),
                    TotalPrice = 0,
                    CustomerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                };
            }

            // Thêm sản phẩm vào giỏ hàng
            var newItem = new OrderItemModel
            {
                ProductId = product.Prod.Id,
                ProductName = product.Prod.Name,
                Price = product.Prod.GetPrice(),
                PublisherName = product.Prod.UserName,
                Quantity = 1,
                ImageUrl = product.Prod.Links.FirstOrDefault(link => link.Url.Contains("cloudinary"))?.Url
            };

            // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
            var existingItem = cart.Order.Items.FirstOrDefault(x => x.ProductId == newItem.ProductId);
            if (existingItem != null)
            {
                // Nếu đã tồn tại, tăng số lượng
                existingItem.Quantity = 1;
            }
            else
            {
                // Nếu chưa, thêm mới vào danh sách
                cart.Order.Items.Add(newItem);
            }

            // Cập nhật tổng giá
            cart.Order.TotalPrice = cart.Order.Items.Sum(x => x.Price * x.Quantity);

            // Lưu giỏ hàng vào cookie
            string cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Response.Cookies.Append("cart", cartJson, new CookieOptions
            {
                HttpOnly = true, // Bảo vệ cookie không bị truy cập bởi JavaScript
            });

            // Trả về dữ liệu giỏ hàng dạng JSON
            return View(cart);
        }




        public IActionResult CartRemoveProduct(string productId)
        {
            string cartJsonCookie = HttpContext.Request.Cookies["cart"];
            CartModel cart = new();

            if (!string.IsNullOrEmpty(cartJsonCookie))
            {
                // Chuyển đổi JSON thành đối tượng CartModel
                cart = JsonConvert.DeserializeObject<CartModel>(cartJsonCookie);
            }
            var itemDelele = cart.Order.Items.FirstOrDefault(id => id.ProductId == productId);
            if (itemDelele != null)
                cart.Order.Items.Remove(itemDelele);

            cart.Order.TotalPrice = cart.Order.Items.Sum(x => x.Price * x.Quantity);

            // Lưu giỏ hàng vào cookie
            string cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Response.Cookies.Append("cart", cartJson, new CookieOptions
            {
                HttpOnly = true // Bảo vệ cookie không bị truy cập bởi JavaScript
            });

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult PasswordSecurity()
        {
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

            if (token == null || !isLogin)
            {
                TempData["error"] = "Session expired, please login again.";
                return View(nameof(Login));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordSecurity(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            model.Id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            string email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
            string username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;

            var responseChangePass = await _authenService.ChangePasswordAsync(model);

            if (responseChangePass.IsSuccess)
            {
                var responseSendMail = await _helperService.SendMail(new Models.SendMailModel()
                {
                    Email = email,
                    Subject = "Change password was success",
                    Body = $"Account with username {username} was change password at {DateTime.Today}"
                });
                if (responseSendMail.IsSuccess)
                    TempData["success"] = "Change password was success, email was send to you";
            }
            else
            {
                TempData["error"] = responseChangePass.Message;
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
        public async Task<IActionResult> UpdateProduct(UpdateProductModel updateProductModel, string SerializedLinks)
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
                    if (!string.IsNullOrEmpty(SerializedLinks))
                    {
                        updateProductModel.Links = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LinkModel>>(SerializedLinks);
                    }
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
                   numOfView, numOfLike, numOfDisLike, updateProductModel.Discount,
                    updateProductModel.Links, updateProductModel.Categories, (int)updateProductModel.Platform,
                    (int)updateProductModel.Status, updateProductModel.CreatedAt, updateProductModel.ImageFiles,
                    request, updateProductModel.UserName, updateProductModel.Interactions.UserLikes, updateProductModel.Interactions.UserDisLikes);

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
                //updateProductModel.Links = model.Links;

                List<LinkModel> listLinks = new List<LinkModel>();
                if (model.Links != null)
                {
                    foreach (var item in model.Links)
                    {
                        listLinks.Add(item);
                    }
                }
                updateProductModel.Links = listLinks;

                //if (model.Links != null)
                //{
                //    List<string> filesImage = new List<string>();
                //    List<IFormFile> filesGame = new List<IFormFile>();
                //    foreach (var link in model.Links)
                //    {
                //        if (link.ProviderName.Contains("Cloudinary"))
                //        {
                //            // Sử dụng trong CreateProductModel
                //            var file = await DownloadFileAsIFormFile(link.Url);
                //            updateProductModel.ImageFiles.Add(file);
                //            filesImage.Add(link.Url);
                //            ViewBag.ImageFiles = filesImage;
                //        }
                //        else if (link.ProviderName.Contains("Google Drive"))
                //        {
                //            var file = await DownloadFileAsIFormFile(link.Url);
                //            updateProductModel.gameFile = file;
                //            //ViewBag.GameFileName = file.FileName;
                //            //ViewBag.GameFileSize = file.Length;
                //            filesGame.Add(file);
                //            ViewBag.Games = filesGame;
                //        }
                //    }
                //}

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
                if (TempData["updateProductModel"] != null)
                {
                    updateProductModel = JsonConvert.DeserializeObject<UpdateProductModel>(TempData["UpdateProductModel"].ToString());
                }

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

                if (updateProductModel.Categories.Count <= 0)
                {
					updateProductModel.Categories.Add(listCate[0]);
				}

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
                    TempData["updateProductModel"] = JsonConvert.SerializeObject(updateProductModel);
                    return RedirectToAction(nameof(UploadProduct));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(UserDashboard));
            }
        }

        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                ResponseModel? response = await _productService.DeleteProductAsync(id);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product deleted successfully";
                    return RedirectToAction(nameof(UserDashboard));
                }
                else
                {
                    throw new Exception(response?.Message);
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

            if (token == null || !isLogin)
            {
                TempData["error"] = "Session expired, please login again.";
                return View(nameof(Login));
            }
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            string i = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            ResponseModel? ProResponese = await _productService.GetAllProductsByUserName(un);
            /*ResponseModel? WishListResponse = await _userService.GetAllProductsInWishList(i);*/
            /*ResponseModel? response2 = await _userService.GetUserAsync(i);*/

            // Kiểm tra và gán oderitem nếu ItemResponse thành công
            if (ProResponese != null && ProResponese.IsSuccess)
            {
                productViewModel.Product = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(ProResponese.Result))
                    ?? new List<ProductModel>();
            }
            else
            {
                // Nếu không có ItemResponse hợp lệ, gán danh sách trống
                productViewModel.Product = new List<ProductModel>();
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
