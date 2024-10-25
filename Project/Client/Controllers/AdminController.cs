using Client.Models;
using Client.Models.CategorisDTO;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Models.AuthenModel;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using ProductModel = Client.Models.ProductDTO.ProductModel;


namespace Client.Controllers
{

    public class AdminController(IUserService userService, IHelperService helperService, IRepoProduct repoProduct, ITokenProvider tokenProvider, ICategoriesService categoryService) : Controller
    {
        #region declaration and initialization
        public readonly IUserService _userService = userService;
        public readonly IHelperService _helperService = helperService;
        public readonly ITokenProvider _tokenProvider = tokenProvider;
        public readonly IRepoProduct _productService = repoProduct;
		public readonly ICategoriesService _categoryService = categoryService;


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
        public async Task<IActionResult> UsersManager()
        {
            UserViewModel users = new();
            try
            {
                ResponseModel? response = await _userService.GetAllUserAsync();

                if (response != null && response.IsSuccess)
                {

                    users.Users = JsonConvert.DeserializeObject<ICollection<UsersDTO>>(Convert.ToString(response.Result.ToString()!));

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


        public async Task<IActionResult> UserDelete(string id)
        {
            ResponseModel? response = await _userService.GetUserAsync(id);

            if (response != null && response.IsSuccess)
            {
                UsersDTO? model = JsonConvert.DeserializeObject<UsersDTO>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }
        public async Task<IActionResult> UserUpdate(string id)
        {
            ResponseModel? response = await _userService.GetUserAsync(id);

            if (response != null && response.IsSuccess)
            {
                UpdateUser? user = JsonConvert.DeserializeObject<UpdateUser>(Convert.ToString(response.Result));

                // Trả về model UsersDTO để sử dụng trong View
                return View(user);
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
                    Avatar = user.Avatar
                    // Nếu bạn có thêm thuộc tính, hãy thêm vào đây
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
            return View(user);

        }

        #endregion



        public async Task<IActionResult> ProductsManager()

        {
            ProductViewModel product = new();
            try
            {
                ResponseModel? response = await _productService.GetAllProductAsync();

                if (response != null && response.IsSuccess)
                {

                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));

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

            return View(product);
        }
        public async Task<IActionResult> UpdateProduct(string id)
        {
            ResponseModel? response = await _productService.GetProductByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                ResponseResultModel<ProductModel>? data =
                    JsonConvert.DeserializeObject<ResponseResultModel<ProductModel>>(Convert.ToString(response.Result));

                // Lấy dữ liệu từ trường "result" và gán vào model
                ProductModel? model = data?.result;

                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct( UpdateProductModel model)
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








        //Delete Product
        public async Task<IActionResult> ProductDelete(string id)
    {
        ResponseModel? response = await _productService.GetProductByIdAsync(id);

        if (response != null && response.IsSuccess)
        {
            ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
            return View(model);
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return NotFound();
    }

    //[HttpPost]
    //public async Task<IActionResult> ProductDelete(ProductModel product)
    //{
    //    ResponseModel? response = await _productService.DeleteProductAsync(product.Id);

    //    if (response != null && response.IsSuccess)
    //    {
    //        TempData["success"] = "Coupon deleted successfully";
    //        return RedirectToAction(nameof(ProductsManager));
    //    }
    //    else
    //    {
    //        TempData["error"] = response?.Message;
    //    }
    //    return View(product);
    //}


    public IActionResult OrdersManager()
        {
            return View();
        }



        public async Task<IActionResult> CategoriesManager()
        {
            CategoriesViewModel categories = new();
            try
            {
                ResponseModel? response = await _categoryService.GetAllCategoryAsync();

                if (response != null && response.IsSuccess)
                {

                    categories.Categories = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response.Result.ToString()!));

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
            if (ModelState.IsValid)
            {
                // Ghi lại thông tin trước khi gửi
                Console.WriteLine("Model is valid. Sending request: " + JsonConvert.SerializeObject(model));

                ResponseModel? response = await _categoryService.CreateCategoryAsync(model);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Category created successfully";
                    return RedirectToAction(nameof(CategoriesManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Failed to create category.";
                }
            }
            else
            {
                // Ghi lại thông tin lỗi
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["error"] = string.Join(", ", errors);
            }

            return RedirectToAction(nameof(CategoriesManager));
        }










        public IActionResult CensorshipManager()
        {
            return View();
        }
    }
}
