using CategoryMicroservice.Repositories.Services;
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


namespace Client.Controllers
{

    public class AdminController(IUserService userService, IHelperService helperService, IRepoProduct repoProduct, ITokenProvider tokenProvider, ICategoriesService categoryService, IOrderService orderService, ICommentService commentService) : Controller
    {
        #region declaration and initialization
        public readonly IUserService _userService = userService;
        public readonly IHelperService _helperService = helperService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoryService = categoryService;
        public readonly ICommentService _commentService = commentService;

        private readonly IOrderService _orderService = orderService;

        #endregion
        public IActionResult Index()

        {
            return View();
        }


        public IActionResult AdminDashboard()
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


        #region User

        [HttpGet]
        public async Task<IActionResult> UsersManager(int? page, int pageSize = 5)
        {
            if (TempData.ContainsKey("searchResult"))
            {
                var userJson = TempData["searchResult"] as string;
                if (userJson == "User not found")
                {
                    UserViewModel userViewModel = new()
                    {
                        CreateUser = new CreateUser(),
                    };
                    TempData["success"] = $"Not found any user";
                    return View(userViewModel);
                }
                if (!string.IsNullOrEmpty(userJson))
                {
                    // Convert JSON thành OrderModel
                    var userSearch = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(userJson);
                    UserViewModel userViewModel = new()
                    {
                        CreateUser = new CreateUser(),
                        Users = userSearch
                    };
                    var data = userSearch;
                    userViewModel.pageNumber = 1;
                    userViewModel.totalItem = data.Count;
                    userViewModel.pageSize = 99;
                    userViewModel.pageCount = (int)Math.Ceiling(userSearch.Count / (double)pageSize);

                    return View(userViewModel);
                }
            }


            int pageNumber = (page ?? 1);
            UserViewModel users = new()
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
                    users.Users = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response.Result.ToString()!));
                    var data = users.Users;
                    users.pageNumber = pageNumber;
                    users.totalItem = data.Count;
                    users.pageSize = pageSize;
                    users.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
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

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> UserCreate(CreateUser user)
        {
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
                // Trả về model UsersDTO để sử dụng trong View
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
            UserViewModel userViewModel = new() { UpdateUser = user };
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
                    // Nếu bạn có thêm thuộc tính, hãy thêm vào đây
                };

                if (user.Role == Models.Enum.UserEnum.User.UserRole.Admin)
                {
                    TempData["error"] = "Can not change role admin";
                    return View(user);
                }

                if (user.Role == Models.Enum.UserEnum.User.UserRole.Admin && user.Status == Models.Enum.UserEnum.User.UserStatus.Active)
                {
                    TempData["error"] = "Can not change status of role admin";
                    return View(user);
                }


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


        public async Task<IActionResult> SearchUsers(string query)
        {
            try
            {
                // Gọi phương thức FindUser để tìm kiếm người dùng theo query
                var response = await _userService.FindUsers(query);

                // Kiểm tra xem tìm kiếm có thành công và trả về kết quả không
                if (response.IsSuccess && response.Result == null)
                {
                    TempData["searchResult"] = "User not found";
                    return RedirectToAction(nameof(UsersManager));
                }

                if (response.IsSuccess && response.Result != null)
                {
                    var users = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(response.Result.ToString());
                    TempData["searchResult"] = JsonConvert.SerializeObject(users);

                    if (users.Count == 1)
                        TempData["success"] = $"Was found 1 user";
                    else
                    {
                        TempData["success"] = $"Was found {users.Count} users";
                    }
                    return RedirectToAction(nameof(UsersManager));
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

            // Nếu có lỗi hoặc không tìm thấy người dùng, chuyển hướng đến trang quản lý người dùng
            return RedirectToAction(nameof(UsersManager));
        }


        public async Task<IActionResult> FilterByUserStatus(UserStatus status)
        {
            return await FilterUsers(u => u.Status == status, $"User status '{status}' not found", $"User status '{status}'");
        }

        public async Task<IActionResult> FilterByEmailStatus(EmailStatus status)
        {
            return await FilterUsers(u => u.EmailConfirmation == status, $"Email status '{status}' not found", $"Email status '{status}'");
        }


        private async Task<IActionResult> FilterUsers(Func<UsersDTO, bool> predicate, string errorMessage, string statusDescription)
        {
            try
            {
                var response = await _userService.GetAllUserAsync(1, 99);

                if (response.IsSuccess && response.Result != null)
                {
                    var users = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(response.Result.ToString())
                                   .Where(predicate)
                                   .ToList();

                    TempData["searchResult"] = JsonConvert.SerializeObject(users);
                    TempData["success"] = $"Filter user with {statusDescription} is successful";
                    return RedirectToAction(nameof(UsersManager));
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

            return RedirectToAction(nameof(UsersManager));
        }



        #endregion


        #region Product

        public async Task<IActionResult> ProductsManager(int? page, int pageSize = 5)

        {
            int pageNumber = (page ?? 1);
            ProductViewModel product = new();
            try
            {

                ResponseModel? response1 = await _categoryService.GetAllCategoryAsync(1, 99);

                ResponseModel? response = await _productService.GetAllProductAsync(pageNumber, pageSize);

                ResponseModel? response2 = await _productService.GetAllProductAsync(1, 99);


                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {

                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));

                    product.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response1.Result.ToString()!));

                    var data = product.Product;
                    product.pageNumber = pageNumber;
                    product.totalItem = data.Count;
                    product.pageSize = pageSize;
                    product.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);

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
            foreach (var item in product.Product)
            {
                string qrCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.QrCode = _productService.GenerateQRCode(qrCodeUrl); // Tạo mã QR và lưu vào thuộc tính

                /*string barCodeUrl = Url.Action("UpdateProduct", "Admin", new { id = item.Id }, Request.Scheme);
                item.BarCode = _productService.GenerateBarCode(11111111111); // Tạo mã QR và lưu vào thuộc tính*/
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

                // Tạo đối tượng ScanFileRequest
                var request = new ScanFileRequest
                {
                    gameFile = model.gameFile
                };

                // Gọi service UpdateProduct từ phía Client
                var response = await _productService.UpdateProductAsync(
                    model.Id, model.Name, model.Description, model.Price, model.Sold,
                   numOfView, numOfLike, model.Discount,
                    model.Links, model.Categories, (int)model.Platform,
                    (int)model.Status, model.CreatedAt, model.ImageFiles,
                    request, model.UserName);

                if (response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
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
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
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
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));
                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
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
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));
                if (response != null && response.IsSuccess)
                {
                    productViewModel.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
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

            return View("ProductsManager", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã lọc
        }


        #endregion


        #region Order 
        public async Task<IActionResult> OrdersManager()
        {
            try
            {
                // Nếu có dữ liệu tìm kiếm, lấy từ TempData
                if (TempData.ContainsKey("searchResult"))
                {
                    var orderJson = TempData["searchResult"] as string;
                    if (!string.IsNullOrEmpty(orderJson))
                    {
                        // Convert JSON thành OrderModel
                        var order = JsonConvert.DeserializeObject<ICollection<OrderModel>>(orderJson);
                        OrderViewModel orderViewModel = new()
                        {
                            Orders = order
                        };
                        return View(orderViewModel);
                    }
                }

                // Nếu không có dữ liệu tìm kiếm, lấy tất cả đơn hàng
                ResponseModel response = await _orderService.GetAll();
                if (response.IsSuccess)
                {
                    var orders = JsonConvert.DeserializeObject<ICollection<OrderModel>>(Convert.ToString(response.Result)!);
                    OrderViewModel orderViewModel = new()
                    {
                        Orders = orders
                    };
                    return View(orderViewModel);
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



        public async Task<IActionResult> SearchOrder(string id)
        {
            try
            {
                var response = await _orderService.GetOrderById(id);
                if (response.Result == null)
                {
                    response = await _orderService.GetOrderByTransaction(id);
                }
                if (response.IsSuccess && response.Result != null)
                {
                    TempData["searchResult"] = JsonConvert.SerializeObject(response.Result);
                    return RedirectToAction(nameof(OrdersManager));
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
                ResponseModel response = await _orderService.GetOrderById(id);

                if (response.IsSuccess)
                {
                    var order = JsonConvert.DeserializeObject<OrderModel>(Convert.ToString(response.Result.ToString()!));
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

        #endregion


        #region Category
        public async Task<IActionResult> CategoriesManager(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            CategoriesViewModel categories = new();
            try
            {
                ResponseModel? response = await _categoryService.GetAllCategoryAsync(pageNumber, pageSize);
                ResponseModel? response2 = await _categoryService.GetAllCategoryAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response2.Result.ToString()!));


                if (response != null && response.IsSuccess)
                {
                    categories.Categories = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = categories.Categories;
                    categories.pageNumber = pageNumber;
                    categories.totalItem = data.Count;
                    categories.pageSize = pageSize;
                    categories.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
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
        public async Task<IActionResult> CommentManager(int? page, int pageSize = 5)
        {
            int pageNumber = (page ?? 1);
            CommentViewModel comment = new();
            try

            {
                ResponseModel? response = await _commentService.GetAllCommentAsync(pageNumber, pageSize);
                ResponseModel? response2 = await _commentService.GetAllCommentAsync(1, 99);
                var total = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(response2.Result.ToString()!));
                if (response != null && response.IsSuccess)
                {
                    comment.Comment = JsonConvert.DeserializeObject<ICollection<CommentDTOModel>>(Convert.ToString(response.Result.ToString()!));
                    var data = comment.Comment;
                    comment.pageNumber = pageNumber;
                    comment.totalItem = data.Count;
                    comment.pageSize = pageSize;
                    comment.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);

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
        [HttpPost]
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

        }

        public async Task<IActionResult> UpdateComment(string id)
        {
            ResponseModel? response = await _commentService.GetByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                CommentDTOModel? model = JsonConvert.DeserializeObject<CommentDTOModel>(Convert.ToString(response.Result));

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
        public async Task<IActionResult> CommentDelete(string id)
        {
            ResponseModel? response = await _commentService.GetListByIdAsync(id);
            if (response != null && response.IsSuccess)
            {
                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));

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
        public async Task<IActionResult> CommentDeleteConfirmed(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                TempData["error"] = "Không có ID nào để xóa.";
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

            TempData["success"] = "Xóa bình luận thành công.";
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
        #endregion
    }
}
