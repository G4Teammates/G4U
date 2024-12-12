using Client.Repositories.Services;
using Client.Models;
using Client.Models.CategorisDTO;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Models.AuthenModel;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

using System.IdentityModel.Tokens.Jwt;

using ProductModel = Client.Models.ProductDTO.ProductModel;
using System.Drawing.Printing;
using Client.Models.ComentDTO;
using Client.Repositories.Interfaces.Comment;

using Client.Repositories.Interfaces.Order;
using Client.Models.OrderModel;
using static Client.Models.Enum.UserEnum.User;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Client.Repositories.Interfaces.Stastistical;
using Client.Models.Statistical;
using System.Security.Claims;
using Client.Models.Enum.OrderEnum;
using Azure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Client.Repositories.Interfaces.Reports;
using Client.Repositories.Services.Reports;
using Client.Models.ReportDTO;



namespace Client.Controllers
{

    public class AdminController(IUserService userService, IHelperService helperService, IRepoProduct repoProduct, ITokenProvider tokenProvider, ICategoriesService categoryService, IOrderService orderService, ICommentService commentService, IRepoStastistical repoStatistical, IReportsService reportsService) : Controller
    { 
        #region declaration and initialization
        public readonly IUserService _userService = userService;
        public readonly IHelperService _helperService = helperService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoryService = categoryService;
        public readonly ICommentService _commentService = commentService;
        public readonly IReportsService _reportService = reportsService;
        private readonly IOrderService _orderService = orderService;
        private readonly IRepoStastistical _statisticalService = repoStatistical;

        #endregion
        public IActionResult Index()

        {
            return View();
        }


        //public IActionResult AdminDashboard()
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
        #region Admindasboard
        public async Task<IActionResult> AdminDashboard(int? page, int pageSize = 99)
        {
            int pageNumber = page ?? 1;
            AllModel statistical = new();
            try
            {
                #region Check IsLogin Cookie and Token

                var isLogin = HttpContext.Request.Cookies["IsLogin"];
                if (string.IsNullOrEmpty(isLogin))
                {
                    ViewData["IsLogin"] = false;
                    TempData["error"] = "Please login first";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["IsLogin"] = isLogin;
                }

                // Lấy token và kiểm tra tính hợp lệ, nhưng luôn tiếp tục để lấy sản phẩm
                var token = _tokenProvider.GetToken();
                var response = _helperService.CheckAndReadToken(token);
                if (response.IsSuccess)
                {
                    var user = _helperService.GetUserFromJwtToken((JwtSecurityToken)response.Result);
                    ViewBag.User = user;
                    ViewData["IsLogin"] = true;
                    if (user.Role == "Admin")
                    {
                        TempData["success"] = "Welcome to admin dashboarch " + user.Username;
                        #region Lấy dữ liệu Order
                        // Gọi API để lấy danh sách Order dựa trên phân trang
                        var responseModel = await _statisticalService.GetAll(pageNumber, pageSize);
                        // Gọi API một lần nữa để lấy tổng số Order (không phân trang)
                        var totalProductsResponse = await _statisticalService.GetAll(1, 99);

                        if (responseModel != null && responseModel.IsSuccess)
                        {
                            // Đọc và gán dữ liệu sản phẩm cho model
                            statistical.statis = JsonConvert.DeserializeObject<ICollection<StatisticalModel>>(responseModel.Result.ToString()!);
                            var totalProducts = JsonConvert.DeserializeObject<ICollection<StatisticalModel>>(totalProductsResponse.Result.ToString()!);

                            statistical.pageNumber = pageNumber;
                            statistical.totalItem = totalProducts.Count;
                            statistical.pageSize = pageSize;
                            statistical.pageCount = (int)Math.Ceiling(totalProducts.Count / (double)pageSize);
                        }
                        else
                        {
                            TempData["error"] = responseModel?.Message;
                        }
                        #endregion
                    }
                    else if (user.Role == "User")
                    {
                        TempData["error"] = "You are not Admin";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewData["IsLogin"] = false;

                }
                #endregion


            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(statistical);
        }
        public async Task<IActionResult> StastistiicalByUser(TotalGroupByUserRequest totalGroupByUserRequest)
        {
            ResponseModel? response = await _statisticalService.GetByUser(totalGroupByUserRequest);

            if (response != null && response.IsSuccess)
            {
                TotalGroupByUserResponse? Model = JsonConvert.DeserializeObject<TotalGroupByUserResponse>(Convert.ToString(response.Result));
                TempData["success"] = "successfully";
                /*ViewBag.Comments = commentsModel;*/  // Gán dữ liệu vào ViewBag
                return PartialView("_UserStatisticsPartial", Model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        #endregion

        #region User

        [HttpGet]
        public async Task<IActionResult> UsersManager(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            UserViewModel userViewModel = new()
            {
                CreateUser = new CreateUser()
            };
            try
            {
                ResponseModel? response = await _userService.GetAllUserAsync(pageNumber, pageSize);
                ResponseModel? response2 = await _userService.GetAllUserAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    userViewModel.Users = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response.Result.ToString()!));
                    var data = userViewModel.Users;
                    userViewModel.pageNumber = pageNumber;
                    userViewModel.totalItem = data.Count;
                    userViewModel.pageSize = pageSize;
                    userViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Load user is success";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(userViewModel);
        }
        //[HttpPost]
        //public async Task<IActionResult> UsersManager(ICollection<UsersDTO> users, int? page, int pageSize = 5)
        //{
        //    int pageNumber = (page ?? 1);
        //    UserViewModel userViewModel = new()
        //    {
        //        CreateUser = new CreateUser(),
        //        Users = users
        //    };
        //    try
        //    {
        //        ResponseModel? response2 = await _userService.GetAllUserAsync(1, 99);
        //        var total = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response2.Result.ToString()!));

        //        var data = userViewModel.Users;
        //        userViewModel.pageNumber = pageNumber;
        //        userViewModel.totalItem = data.Count;
        //        userViewModel.pageSize = pageSize;
        //        userViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
        //        TempData["success"] = "Load user is success";



        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = ex.Message;
        //    }

        //    return View(userViewModel);
        //}


        [HttpPost]
        public async Task<IActionResult> UserCreate(CreateUser user)
        {
            user.Avatar ??= "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
            if (ModelState.IsValid)
            {
                if (user.AvatarFile != null && user.AvatarFile.Length > 0)
                {
                    using (var stream = user.AvatarFile.OpenReadStream())
                    {
                        // Gọi phương thức upload lên Cloudinary
                        string imageUrl = await _helperService.UploadImageAsync(stream, user.AvatarFile.FileName);

                        // Lưu URL của avatar vào model
                        user.Avatar = imageUrl;
                    }
                }

                ResponseModel? response = await _userService.CreateUserAsync(user);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "User created successfully";
                    return RedirectToAction(nameof(UsersManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }

            }
            return RedirectToAction(nameof(UsersManager));
        }

        [HttpGet]
        public async Task<IActionResult> UserUpdate(string id)
        {
            ResponseModel? response = await _userService.GetUserAsync(id);

            if (response != null && response.IsSuccess)
            {
                UpdateUser? user = JsonConvert.DeserializeObject<UpdateUser>(Convert.ToString(response.Result));
                UserViewModel userViewModel = new()
                {
                    UpdateUser = user
                };

                // Use TempData to persist the role across redirects
                TempData["role"] = user.Role.ToString();
                TempData["success"] = "Get user need update success";

                return PartialView("UserUpdate", user);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> UserUpdate(UpdateUser user)
        {
            UserViewModel userViewModel = new()
            {
                UpdateUser = user
            };

            // Get the role from TempData instead of ViewData
            var roleFromTempData = TempData["role"]?.ToString();

            if (user.Role != UserRole.Admin && roleFromTempData == UserRole.Admin.ToString())
            {
                TempData["error"] = "Can not change role admin";
                return RedirectToAction(nameof(UsersManager));
            }

            if (user.Role == UserRole.Admin && user.Status != UserStatus.Active && roleFromTempData == UserRole.Admin.ToString())
            {
                TempData["error"] = "Can not change status of role admin";
                return RedirectToAction(nameof(UsersManager));
            }

            if (user.AvatarFile != null && user.AvatarFile.Length > 0)
            {
                using (var stream = user.AvatarFile.OpenReadStream())
                {
                    // Gọi phương thức upload lên Cloudinary
                    string imageUrl = await _helperService.UploadImageAsync(stream, user.AvatarFile.FileName);

                    // Lưu URL của avatar vào model
                    user.Avatar = imageUrl;
                }
            }

            if (ModelState.IsValid)
            {
                var updateUser = new UpdateUser
                {
                    Id = user.Id,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    Avatar = user.Avatar
                };

                ResponseModel? response = await _userService.UpdateUser(updateUser);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "User updated successfully";
                    return RedirectToAction(nameof(UsersManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

            // Nếu ModelState không hợp lệ, trả về lại model để hiển thị lỗi
            return RedirectToAction(nameof(UsersManager));
        }


        [HttpGet]
        public async Task<IActionResult> UserDelete(string id)
        {
            ResponseModel? response = await _userService.GetUserAsync(id);

            if (response != null && response.IsSuccess)
            {
                UsersDTO? model = JsonConvert.DeserializeObject<UsersDTO>(Convert.ToString(response.Result));
                TempData["success"] = "Get user need delete success";
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(UsersDTO user)
        {
            if (user.Role != UserRole.Admin)
            {
                ResponseModel? response = await _userService.ChangeStatus(user.Id, UserStatus.Deleted);

                if (response != null && response.IsSuccess)
                {
                    UsersDTO? model = JsonConvert.DeserializeObject<UsersDTO>(Convert.ToString(response.Result));
                    TempData["success"] = "Delete user is success";
                    return RedirectToAction(nameof(UsersManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            TempData["error"] = "Can not deleted Admin";
            return RedirectToAction(nameof(UsersManager));
        }


        //public async Task<IActionResult> SearchUsers(string query, int? page, int pageSize = 5)
        //{
        //    int pageNumber = page ?? 1;
        //    UserViewModel userViewModel = new() { CreateUser = new CreateUser() };

        //    try
        //    {
        //        var response = await _userService.FindUsers(query, 1, 999);

        //        if (response.IsSuccess && response.Result != null)
        //        {
        //            var allUsers = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(response.Result.ToString());

        //            // Phân trang
        //            (userViewModel.pageCount, userViewModel.Users) = Paginate(allUsers, pageNumber, pageSize);

        //            userViewModel.pageNumber = pageNumber;
        //            userViewModel.pageSize = pageSize;
        //            userViewModel.totalItem = allUsers.Count;

        //            TempData["success"] = $"Found {allUsers.Count} user(s) matching '{query}'";
        //            return View(nameof(UsersManager), userViewModel);
        //        }
        //        else
        //        {
        //            TempData["error"] = response.Message ?? "User not found";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = ex.Message;
        //    }

        //    return View(nameof(UsersManager), userViewModel);
        //}

        // Prepare to search users
        public async Task<IActionResult> PrepareSearchUsers(string query, int? page, int pageSize = 5)
        {
            ViewData["FilterUserAction"] = nameof(SearchUsers);
            return await SearchUsers(query, page, pageSize);
        }

        // Search users by query
        public async Task<IActionResult> SearchUsers(string query, int? page, int pageSize = 5)
        {
            ViewData["FilterUserAction"] = nameof(SearchUsers);

            int pageNumber = page ?? 1;
            UserViewModel userViewModel = new() 
            { 
                CreateUser = new CreateUser(),
                Users = new List<UsersDTO>()
            };

            try
            {
                var response = await _userService.FindUsers(query, 1, 999);

                if (response.IsSuccess && response.Result != null)
                {
                    var allUsers = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(response.Result.ToString());

                    // Paginate results
                    (userViewModel.pageCount, userViewModel.Users) = Paginate(allUsers, pageNumber, pageSize);

                    userViewModel.pageNumber = pageNumber;
                    userViewModel.pageSize = pageSize;
                    userViewModel.totalItem = allUsers.Count;

                    TempData["success"] = $"Found {allUsers.Count} user(s) matching '{query}'";
                    return View(nameof(UsersManager), userViewModel);
                }
                else
                {
                    TempData["error"] = response.Message ?? "User not found";
                    userViewModel.pageNumber = pageNumber;
                    userViewModel.pageSize = pageSize;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(nameof(UsersManager), userViewModel);
        }

        // Filter users by status
        public async Task<IActionResult> FilterByUserStatus(UserStatus status, int? page, int pageSize = 5)
        {
            ViewData["FilterUserAction"] = nameof(FilterByUserStatus);
            return await FilterUsers(u => u.Status == status, $"User status '{status}' not found", $"User status '{status}'", page, pageSize);
        }

        // Filter users by email status
        public async Task<IActionResult> FilterByEmailStatus(EmailStatus status, int? page, int pageSize = 5)
        {
            ViewData["FilterUserAction"] = nameof(FilterByEmailStatus);
            return await FilterUsers(u => u.EmailConfirmation == status, $"Email status '{status}' not found", $"Email status '{status}'", page, pageSize);
        }
        // Filter users by role
        public async Task<IActionResult> FilterByRole(UserRole status, int? page, int pageSize = 5)
        {
            ViewData["FilterUserAction"] = nameof(FilterByRole);
            return await FilterUsers(u => u.Role == status, $"Role '{status}' not found", $"Role '{status}'", page, pageSize);
        }

        // Generic method to filter users based on a predicate
        private async Task<IActionResult> FilterUsers(Func<UsersDTO, bool> predicate, string errorMessage, string statusDescription, int? page, int pageSize)
        {
            int pageNumber = page ?? 1;
            UserViewModel userViewModel = new()
            {
                CreateUser = new CreateUser(),
                Users = new List<UsersDTO>()
            };

            try
            {
                var response = await _userService.GetAllUserAsync(1, 999); // Get all users

                if (response.IsSuccess && response.Result != null)
                {
                    var allUsers = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(response.Result.ToString())
                        .Where(predicate)
                        .ToList();

                    // Paginate results
                    (userViewModel.pageCount, userViewModel.Users) = Paginate(allUsers, pageNumber, pageSize);

                    userViewModel.pageNumber = pageNumber;
                    userViewModel.pageSize = pageSize;
                    userViewModel.totalItem = allUsers.Count;

                    TempData["success"] = $"Filter user with {statusDescription} is successful";
                    return View(nameof(UsersManager), userViewModel);
                }
                else
                {
                    TempData["error"] = response.Message ?? errorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(nameof(UsersManager), userViewModel);
        }

        // Paginate data (for users)
        private (int pageCount, ICollection<UsersDTO> paginatedData) Paginate(ICollection<UsersDTO> data, int pageNumber, int pageSize)
        {
            int totalItems = data.Count;
            int pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedData = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pageCount, paginatedData);
        }


        #endregion


        #region Product

        public async Task<IActionResult> ProductsManager(int? page, int pageSize = 5)

        {
            int pageNumber = (page ?? 1);
            ProductViewModel product = new();
            try
            {
                // Lấy tất cả danh mục
                ResponseModel? response1 = await _categoryService.GetAllCategoryAsync(1, 99);

                // Lấy dữ liệu của trang hiện tại
                ResponseModel? response = await _productService.GetAllProductAsync(pageNumber, pageSize);

                // Lấy toàn bộ sản phẩm để tính tổng số mục
                ResponseModel? responseTotal = await _productService.GetAllProductAsync(1, int.MaxValue);
                var allProducts = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(responseTotal.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    // Dữ liệu trang hiện tại
                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));

                    // Danh mục sản phẩm
                    product.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response1.Result.ToString()!));

                    product.pageNumber = pageNumber;
                    product.totalItem = allProducts?.Count ?? 0; // Tổng số sản phẩm
                    product.pageSize = pageSize;
                    product.pageCount = (int)Math.Ceiling(product.totalItem / (double)pageSize);
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            // Tạo mã QR cho từng sản phẩm trong trang hiện tại
            foreach (var item in product.Product)
            {
                string qrCodeUrl = Url.Action("ProductDetail", "Product", new { id = item.Id }, Request.Scheme);
                item.QrCode = _productService.GenerateQRCode(qrCodeUrl);
            }

            return View(product);
        }
        public async Task<IActionResult> UpdateProduct(string id)
        {
            ResponseModel? response = await _productService.GetProductByIdAsync(id);

            if (response != null && response.IsSuccess)
            {

                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));

                // Trả về model UsersDTO để sử dụng trong View
                TempData["success"] = "Get product need update successfully";
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        [RequestSizeLimit(60 * 1024 * 1024)] // 50MB
        [RequestFormLimits(MultipartBodyLengthLimit = 60 * 1024 * 1024)] // Đặt giới hạn cho form multipart
        public async Task<IActionResult> UpdateProduct(UpdateProductModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                var numOfView = model.Interactions.NumberOfViews;
                var numOfLike = model.Interactions.NumberOfLikes;
                var numOfDisLike = model.Interactions.NumberOfDisLikes;

                // Tạo đối tượng ScanFileRequest
                var request = new ScanFileRequest
                {
                    gameFile = model.gameFile
                };

                // Gọi service UpdateProduct từ phía Client
                var response = await _productService.UpdateProductAsync(
                    model.Id, model.Name, model.Description, model.Price, model.Sold,
                   numOfView, numOfLike, numOfDisLike, model.Discount,
                    model.Links, model.Categories, (int)model.Platform,
                    (int)model.Status, model.CreatedAt, model.ImageFiles,
                    request, model.UserName, model.Interactions.UserLikes, model.Interactions.UserDisLikes);

                if (response.IsSuccess)
                {
                    TempData["success"] = "Product updated successfully";
                    return RedirectToAction(nameof(ProductsManager));
                }
                else
                {
                    return BadRequest(response.Message); // Trả về lỗi từ service
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        [RequestSizeLimit(60 * 1024 * 1024)] // 50MB
        [RequestFormLimits(MultipartBodyLengthLimit = 60 * 1024 * 1024)] // Đặt giới hạn cho form multipart
        public async Task<IActionResult> CreateProduct(CreateProductModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                model.Username = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                // Tạo đối tượng ScanFileRequest
                var request = new ScanFileRequest
                {
                    gameFile = model.gameFile
                };

                // Gọi API CreateProductAsync
                var response = await _productService.CreateProductAsync(
                    model.Name,
                    model.Description,
                    model.Price,
                    model.Discount,
                    model.Categories,
                    model.Platform,
                    model.Status,
                    model.imageFiles,
                    request,
                    model.Username);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(ProductsManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }


        public async Task<IActionResult> DeleteProduct(string id)
        {
            ResponseModel? response = await _productService.GetProductByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
                TempData["success"] = "Get product for delete successfully";
                // Trả về model UsersDTO để sử dụng trong View
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(ProductModel product)
        {
            ResponseModel? response = await _productService.DeleteProductAsync(product.Id);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully";
                return RedirectToAction(nameof(ProductsManager));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }



        [HttpPost]
        public async Task<IActionResult> SearchProduct(string searchString, int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            ProductViewModel productViewModel = new();

            try
            {
                // Gọi API để tìm kiếm sản phẩm theo từ khóa
                ResponseModel? response = await _productService.SearchProductAsync(searchString, page, pageSize);
                ResponseModel? response2 = await _productService.GetAllProductAsync(1, 99);
                ResponseModel? response3 = await _categoryService.GetAllCategoryAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    productViewModel.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response3.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Search product successfully";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            // Tạo mã QR cho từng sản phẩm
            foreach (var item in productViewModel.Product)
            {
                string qrCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.QrCode = _productService.GenerateQRCode(qrCodeUrl); // Tạo mã QR và lưu vào thuộc tính

                /*string barCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.BarCode = _productService.GenerateBarCode(11111111111); // Tạo mã QR và lưu vào thuộc tính*/
            }


            return View("ProductsManager", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã tìm kiếm
        }

        [HttpPost]
        public async Task<IActionResult> SortProducts(string sort, int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            ProductViewModel productViewModel = new();

            try
            {
                // Gọi API để lấy danh sách sản phẩm đã sắp xếp
                ResponseModel? response = await _productService.SortProductAsync(sort, page, pageSize);
                ResponseModel? response2 = await _productService.GetAllProductAsync(1, 99);
                ResponseModel? response3 = await _categoryService.GetAllCategoryAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));
                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    productViewModel.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Sort product successfully";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            // Tạo mã QR cho từng sản phẩm
            foreach (var item in productViewModel.Product)
            {
                string qrCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.QrCode = _productService.GenerateQRCode(qrCodeUrl); // Tạo mã QR và lưu vào thuộc tính

                /*string barCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.BarCode = _productService.GenerateBarCode(11111111111); // Tạo mã QR và lưu vào thuộc tính*/
            }

            return View("ProductsManager", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã sắp xếp
        }

        [HttpPost]
        public async Task<IActionResult> FilterProducts(
                                                        decimal? minRange,
                                                        decimal? maxRange,
                                                        int? sold,
                                                        bool? discount,
                                                        int? platform,
                                                        string category,
                                                        int? page,
                                                        int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            ProductViewModel productViewModel = new();

            try
            {
                // Gọi API để lọc sản phẩm theo các điều kiện
                ResponseModel? response = await _productService.FilterProductAsync(minRange, maxRange, sold, discount, platform, category, page, pageSize);
                ResponseModel? response2 = await _productService.GetAllProductAsync(1, 99);
                ResponseModel? response3 = await _categoryService.GetAllCategoryAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));
                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    productViewModel.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response3.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Filer product successfully";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            // Tạo mã QR cho từng sản phẩm
            foreach (var item in productViewModel.Product)
            {
                string qrCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.QrCode = _productService.GenerateQRCode(qrCodeUrl); // Tạo mã QR và lưu vào thuộc tính

                /*string barCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.BarCode = _productService.GenerateBarCode(11111111111); // Tạo mã QR và lưu vào thuộc tính*/
            }

            return View("ProductsManager", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã lọc
        }


        #endregion


        #region Order 
        public async Task<IActionResult> OrdersManager(int? page, int pageSize = 5)
        {

            OrderViewModel orders = new();
            int pageNumber = (page ?? 1);
            try
            {
                // Nếu không có dữ liệu tìm kiếm, lấy tất cả đơn hàng
                ResponseModel? response = await _orderService.GetAll(pageNumber, pageSize);
                ResponseModel? response2 = await _orderService.GetAll(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    orders.Orders = JsonConvert.DeserializeObject<ICollection<OrderModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = orders.Orders;
                    orders.pageNumber = pageNumber;
                    orders.totalItem = data.Count;
                    orders.pageSize = pageSize;
                    orders.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Load order is success";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }


            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View(orders);
        }




        [HttpGet]
        public async Task<IActionResult> OrderDetail(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrderItems(id);

                if (response.IsSuccess)
                {
                    var orderItems = JsonConvert.DeserializeObject<ICollection<OrderItemModel>>(Convert.ToString(response.Result.ToString()!));
                    OrderViewModel orderViewModel = new()
                    {
                        Items = orderItems
                    };
                    TempData["success"] = "Get order detail successfully";
                    return PartialView("_OrderItemsPartial", orderViewModel);
                }
                else
                {
                    TempData["error"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string id, PaymentStatusModel status)
        {
            try
            {
                ResponseModel response = await _orderService.UpdateStatus(id, status);

                if (response.IsSuccess)
                {
                    TempData["success"] = "Order status updated successfully";
                }
                else
                {
                    TempData["error"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(OrdersManager));
        }



        [HttpGet]
        public async Task<IActionResult> UpdateOrderStatus(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrderById(id, 1, 1);

                if (response.IsSuccess)
                {
                    ICollection<OrderModel> orders = JsonConvert.DeserializeObject<ICollection<OrderModel>>(Convert.ToString(response.Result.ToString()!));
                    OrderModel order = orders.FirstOrDefault();
                    TempData["success"] = "Get order need to update successfully";
                    return View(order);
                }
                else
                {
                    TempData["error"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(OrdersManager));
        }

        // Phương thức SearchOrder
        public async Task<IActionResult> SearchOrder(string id, int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            OrderViewModel orderViewModel = new()
            {
                Orders = new List<OrderModel>(),
                pageNumber = pageNumber,
                pageSize = pageSize
            };

            try
            {
                // Tìm đơn hàng theo Id hoặc Transaction Id
                var response = await SearchOrderByIdOrTransaction(id);

                if (response.IsSuccess && response.Result != null)
                {
                    var ordersList = JsonConvert.DeserializeObject<ICollection<OrderModel>>(response.Result.ToString());
                    (orderViewModel.pageCount, orderViewModel.Orders) = Paginate(ordersList, pageNumber, pageSize);

                    TempData["success"] = response.Message ?? "Orders fetched successfully.";
                }
                else
                {
                    TempData["error"] = response.Message ?? "No orders found."; 
                    page = 1;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
            }

            return View(nameof(OrdersManager), orderViewModel);
        }

        // Tìm kiếm đơn hàng theo Id hoặc Transaction Id
        private async Task<ResponseModel> SearchOrderByIdOrTransaction(string id)
        {
            var response = await _orderService.GetOrderById(id, 1, 999);
            if (response.Result == null || !response.IsSuccess)
            {
                response = await _orderService.GetOrderByTransaction(id, 1, 999);
            }
            return response;
        }

        // Phương thức FilterOrders (chung cho tất cả bộ lọc)
        public async Task<IActionResult> FilterOrders<T>(
            Func<OrderModel, bool> predicate,
            string successMessage,
            string errorMessage,
            int? page,
            int pageSize)
        {
            int pageNumber = page ?? 1;

            OrderViewModel orderViewModel = new()
            {
                Orders = new List<OrderModel>(),
                pageNumber = pageNumber,
                pageSize = pageSize
            };

            try
            {
                var response = await _orderService.GetAll(1, 999);

                if (response.IsSuccess && response.Result != null)
                {
                    var filteredOrders = JsonConvert.DeserializeObject<ICollection<OrderModel>>(response.Result.ToString())
                        .Where(predicate)
                        .ToList();

                    (orderViewModel.pageCount, orderViewModel.Orders) = Paginate(filteredOrders, pageNumber, pageSize);

                    TempData["success"] = successMessage;
                }
                else
                {
                    TempData["error"] = errorMessage;
                    page = 1;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
            }

            return View(nameof(OrdersManager), orderViewModel);
        }

        // Các phương thức filter cụ thể sử dụng FilterOrders
        public async Task<IActionResult> FilterByOrderStatus(OrderStatus status, int? page, int pageSize = 5)
        {
            ViewData["FilterOrderAction"] = nameof(FilterByOrderStatus);
            return await FilterOrders<OrderModel>(
                o => o.OrderStatus == status,
                $"Filter by order status '{status}' was successful.",
                $"No orders found with status '{status}'.",
                page,
                pageSize);
        }

        public async Task<IActionResult> FilterByPaymentStatus(PaymentStatus status, int? page, int pageSize = 5)
        {
            ViewData["FilterOrderAction"] = nameof(FilterByPaymentStatus);
            return await FilterOrders<OrderModel>(
                o => o.PaymentStatus == status,
                $"Filter by payment status '{status}' was successful.",
                $"No orders found with payment status '{status}'.",
                page,
                pageSize);
        }

        public async Task<IActionResult> FilterByPaymentMethod(PaymentMethod status, int? page, int pageSize = 5)
        {
            ViewData["FilterOrderAction"] = nameof(FilterByPaymentMethod);
            return await FilterOrders<OrderModel>(
                o => o.PaymentMethod == status,
                $"Filter by payment method '{status}' was successful.",
                $"No orders found with payment method '{status}'.",
                page,
                pageSize);
        }

        // Phương thức Paginate
        private (int pageCount, ICollection<T> paginatedItems) Paginate<T>(ICollection<T> items, int pageNumber, int pageSize)
        {
            int totalItems = items.Count;
            int pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedItems = items
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pageCount, paginatedItems);
        }



        #endregion


        #region Category
        public async Task<IActionResult> CategoriesManager(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            CategoriesViewModel categories = new();
            try
            {
                // Lấy dữ liệu của trang hiện tại
                ResponseModel? response = await _categoryService.GetAllCategoryAsync(pageNumber, pageSize);

                // Lấy tất cả dữ liệu (dùng pageSize lớn để tải hết dữ liệu)
                ResponseModel? responseTotal = await _categoryService.GetAllCategoryAsync(1, int.MaxValue);
                var allData = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(responseTotal.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    categories.Categories = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = categories.Categories;

                    categories.pageNumber = pageNumber;
                    categories.totalItem = allData?.Count ?? 0; // Tổng số bản ghi
                    categories.pageSize = pageSize;
                    categories.pageCount = (int)Math.Ceiling(categories.totalItem / (double)pageSize);
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(categories);
        }








        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategories model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                // Gọi service CreateCategoryAsync
                var response = await _categoryService.CreateCategoryAsync(model);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Category created successfully";
                    return RedirectToAction(nameof(CategoriesManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return RedirectToAction(nameof(CategoriesManager));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(CategoriesManager));
            }
        }


        public async Task<IActionResult> UpdateCategory(string id)
        {
            ResponseModel? response = await _categoryService.GetCategoryAsync(id);

            if (response != null && response.IsSuccess)
            {
                CategoriesModel? model = JsonConvert.DeserializeObject<CategoriesModel>(Convert.ToString(response.Result));
                TempData["success"] = "Get category for update successfully";
                // Trả về model UsersDTO để sử dụng trong View
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategoryConfirm([FromForm] CategoriesModel category)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                // Gọi service UpdateCategoryAsync từ ICategoriesService
                var response = await _categoryService.UpdateCategoryAsync(category);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Category updated successfully";
                    return RedirectToAction(nameof(CategoriesManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return BadRequest(response.Message); // Trả về view với dữ liệu đã nhập

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return StatusCode(500); // Trả về view với dữ liệu đã nhập và lỗi
            }
        }

        public async Task<IActionResult> CategoryDelete(string id)
        {
            ResponseModel? response = await _categoryService.GetCategoryAsync(id);
            if (response != null && response.IsSuccess)
            {
                CategoriesModel? model = JsonConvert.DeserializeObject<CategoriesModel>(Convert.ToString(response.Result));
                TempData["success"] = "GEt category for delete successfully";
                // Trả về model UsersDTO để sử dụng trong View
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> CategoryDeleteConfirmed(string id)
        {
            try
            {
                ResponseModel? response = await _categoryService.DeleteCategoryAsync(id);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Category deleted successfully";
                    return RedirectToAction(nameof(CategoriesManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return RedirectToAction(nameof(CategoriesManager));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return StatusCode(500); // Trả về view với dữ liệu đã nhập và lỗi
            }
        }


        [HttpPost]
        public async Task<IActionResult> SearchCategory(string searchString, int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            CategoriesViewModel categoryViewModel = new();

            try
            {
                // Gọi API để tìm kiếm sản phẩm theo từ khóa
                ResponseModel? response = await _categoryService.SearchProductAsync(searchString, page, pageSize);
                ResponseModel? response2 = await _categoryService.GetAllCategoryAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    categoryViewModel.Categories = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = categoryViewModel.Categories;
                    categoryViewModel.pageNumber = pageNumber;
                    categoryViewModel.totalItem = data.Count;
                    categoryViewModel.pageSize = pageSize;
                    categoryViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Search category successfully";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View("CategoriesManager", categoryViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã tìm kiếm
        }

        #endregion


        #region Comment
        public async Task<IActionResult> CommentManager(int? page, int pageSize = 20)
        {
            int pageNumber = (page ?? 1);
            CommentViewModel comment = new();
            try
            {
                // Lấy dữ liệu trang hiện tại
                ResponseModel? response = await _commentService.GetAllCommentAsync(pageNumber, pageSize);

                // Lấy toàn bộ dữ liệu để tính tổng số comment
                ResponseModel? responseTotal = await _commentService.GetAllCommentAsync(1, int.MaxValue);
                var allComments = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(responseTotal.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    comment.Comment = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = comment.Comment;

                    comment.pageNumber = pageNumber;
                    comment.totalItem = allComments?.Count ?? 0; // Tổng số comment
                    comment.pageSize = pageSize;
                    comment.pageCount = (int)Math.Ceiling(comment.totalItem / (double)pageSize);
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(comment);
        }
        /* [HttpPost]
         public async Task<IActionResult> CreateComment(CreateCommentDTOModel model)
         {
             if (!ModelState.IsValid)
             {
                 var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage);
                 return BadRequest(new { Errors = errors });
             }

             try
             {
                 // Gọi service CreateCommentAsync
                 var response = await _commentService.CreateCommentAsync(model);

                 if (response != null && response.IsSuccess)
                 {
                     TempData["success"] = "Category created successfully";
                     return RedirectToAction(nameof(CommentManager));
                 }
                 else
                 {
                     TempData["error"] = response?.Message ?? "An unknown error occurred.";
                     return RedirectToAction(nameof(CommentManager));
                 }
             }
             catch (Exception ex)
             {
                 TempData["error"] = $"An error occurred: {ex.Message}";
                 return RedirectToAction(nameof(CommentManager));
             }

         }*/

        public async Task<IActionResult> UpdateComment(string id)
        {
            ResponseModel? response = await _commentService.GetByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                CommentDTOModel? model = JsonConvert.DeserializeObject<CommentDTOModel>(Convert.ToString(response.Result));
                TempData["success"] = "Get comment for update successfully";
                // Trả về model UsersDTO để sử dụng trong View
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCommentConfirm([FromForm] CommentDTOModel comment)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try

            {
                var response = await _commentService.UpdateCommentAsync(comment);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Category updated successfully";
                    return RedirectToAction(nameof(CommentManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return BadRequest(response.Message); // Trả về view với dữ liệu đã nhập

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return StatusCode(500); // Trả về view với dữ liệu đã nhập và lỗi
            }
        }
        public async Task<IActionResult> CommentDelete(string id, int? page, int pageSize = 999)
        {
            int pageNumber = (page ?? 1);
            ResponseModel? response = await _commentService.GetListByIdAsync(id, pageNumber, pageSize);

            if (response != null && response.IsSuccess)
            {
                List<CommentDTOModel>? commentsModel = JsonConvert.DeserializeObject<List<CommentDTOModel>>(Convert.ToString(response.Result));
                TempData["success"] = "Get comment for update successfully";
                ViewBag.Comments = commentsModel;  // Gán dữ liệu vào ViewBag
                return View();
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> CommentDeleteConfirmed(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                TempData["error"] = "Id is not null.";
                return RedirectToAction("CommentManager");
            }

            var idList = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in idList)
            {
                var response = await _commentService.DeleteCommentAsync(id.Trim());
                if (!response.IsSuccess)
                {
                    TempData["error"] = response.Message; // Ghi nhận lỗi
                    return RedirectToAction("CommentManager");
                }
            }

            TempData["success"] = "Delete Comment successfully.";
            return RedirectToAction("CommentManager");
        }

        [HttpPost]
        public async Task<IActionResult> SearchCmt(string searchString, int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            CommentViewModel cmtViewModel = new();

            try
            {
                // Gọi API để tìm kiếm sản phẩm theo từ khóa
                ResponseModel? response = await _commentService.SearchCmtAsync(searchString, page, pageSize);
                ResponseModel? response2 = await _commentService.GetAllCommentAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    cmtViewModel.Comment = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = cmtViewModel.Comment;
                    cmtViewModel.pageNumber = pageNumber;
                    cmtViewModel.totalItem = data.Count;
                    cmtViewModel.pageSize = pageSize;
                    cmtViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Search comment successfully";
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View("CommentManager", cmtViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã tìm kiếm
        }

        [HttpPost]
        public async Task<IActionResult> AddWishList(WishlistModel wishlist)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            try
            {
                var response = await _userService.AddToWishList(wishlist, un);

                if (response != null && response.IsSuccess)
                {
                    return Json(new { success = true, message = "Add Wishlist successfully" });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "An unknown error occurred." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> RemoveWishList(string productId)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                /*return Json(new { success = false, errors = errors });*/
                TempData["error"] = "ModelState false.";
                return RedirectToAction("Collection", "Product");
            }

            try
            {
                var response = await _userService.RemoveWishList(productId, un);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Remove wishlist item successfully";
                    return RedirectToAction("Collection", "Product");
                }
                else
                {
                    /*return Json(new { success = false, message = response?.Message ?? "An unknown error occurred." });*/
                    TempData["error"] = "An unknown error occurred: " + response.Message;
                    return RedirectToAction("Collection", "Product");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An unknown error occurred: " + ex.Message;
                return RedirectToAction("Collection", "Product");
            }
        }
        #endregion


        #region Report
        public async Task<IActionResult> ReportManager(int? page, int pageSize = 20)
        {
            int pageNumber = (page ?? 1);
            ReportViewModel report = new();
            try
            {
                // Lấy dữ liệu trang hiện tại
                ResponseModel? response = await _reportService.GetAll(pageNumber, pageSize);

                // Lấy toàn bộ dữ liệu để tính tổng số comment
                ResponseModel? responseTotal = await _reportService.GetAll(1, int.MaxValue);
                var allComments = JsonConvert.DeserializeObject<ICollection<ReportsModel>>(Convert.ToString(responseTotal.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    report.Report = JsonConvert.DeserializeObject<ICollection<ReportsModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = report.Report;

                    report.pageNumber = pageNumber;
                    report.totalItem = allComments?.Count ?? 0;
                    report.pageSize = pageSize;
                    report.pageCount = (int)Math.Ceiling(report.totalItem / (double)pageSize);
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportsModels model)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                // Gọi service CreateCommentAsync
                var response = await _reportService.CreateReport(model,un);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Report created successfully";
                    return View("~/Views/G4T/ContactUs.cshtml");
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return View("~/Views/G4T/ContactUs.cshtml");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return View("~/Views/G4T/ContactUs.cshtml");
            }

        }
        public async Task<IActionResult> UpdateReport(string id)
        {
            ResponseModel? response = await _reportService.GetById(id);

            if (response != null && response.IsSuccess)
            {
                ReportsModel? model = JsonConvert.DeserializeObject<ReportsModel>(Convert.ToString(response.Result));
                TempData["success"] = "Get Report for update successfully";
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateReportConfirm(string reportId, int status)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try

            {
                var response = await _reportService.UpdateReport(reportId, status);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Report updated successfully";
                    return RedirectToAction(nameof(ReportManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return BadRequest(response.Message); // Trả về view với dữ liệu đã nhập

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return StatusCode(500); // Trả về view với dữ liệu đã nhập và lỗi
            }
        }
        #endregion
    }
}
