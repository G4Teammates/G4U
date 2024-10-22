using Client.Models;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using ProductModel = Client.Models.ProductDTO.ProductModel;


namespace Client.Controllers
{

    public class AdminController(IUserService userService, IHelperService helperService, IRepoProduct repoProduct) : Controller

    {

        public readonly IUserService _userService = userService;
        public readonly IHelperService _helperService = helperService;

        public readonly IRepoProduct _productService = repoProduct;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

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
                // Kiểm tra xem có avatar không
                if (user.AvatarFile != null && user.AvatarFile.Length > 0)
                {
                    await using (var stream = user.AvatarFile.OpenReadStream())
                    {
                        // Gọi phương thức kiểm duyệt từ _helperService
                        var resultModerate = await _helperService.Moderate(stream);
                        if (!resultModerate.IsSuccess)
                        {
                            TempData["error"] = "Avatar is not safe";
                            return RedirectToAction(nameof(UsersManager));
                        }

                        // Đặt vị trí stream về đầu và upload lên Cloudinary
                        stream.Position = 0;
                        var imageUrl = await _helperService.UploadImageAsync(stream, user.AvatarFile.FileName);

                        // Lưu URL của avatar vào model
                        user.Avatar = imageUrl;
                    }
                }

                // Gọi phương thức tạo user từ _userService
                var response = await _userService.CreateUserAsync(user);

                if (response?.IsSuccess == true)
                {
                    TempData["success"] = "User created successfully";
                    return RedirectToAction(nameof(UsersManager));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Failed to create user";
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

        [HttpPost]
        public async Task<IActionResult> UserDelete(UsersDTO user)
        {
            ResponseModel? response = await _userService.DeleteUser(user.Id);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(UsersManager));
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
        public async Task<IActionResult> UpdateProduct([FromForm] string id,
                                                   [FromForm] string name,
                                                   [FromForm] string description,
                                                   [FromForm] decimal price,
                                                   [FromForm] int sold,
                                                   [FromForm] int numOfView,
                                                   [FromForm] int numOfLike,
                                                   [FromForm] float discount,
                                                   [FromForm] List<string> categories,
                                                   [FromForm] int platform,
                                                   [FromForm] int status,
                                                   [FromForm] DateTime createAt,
                                                   [FromForm] List<IFormFile> imageFiles,
                                                   [FromForm] ScanFileRequest request,
                                                   [FromForm] string username)
        {
            try
            {
                // Gọi service UpdateProduct từ phía Client
                var response = await _productService.UpdateProductAsync(
                    id, name, description, price, sold, numOfView, numOfLike, discount,
                    categories, platform, status, createAt, imageFiles, request, username);

                if (response.IsSuccess)
                {
                    return Ok(response); // Trả về kết quả thành công
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

        public IActionResult CategoriesManager()
        {
            return View();
        }

        public IActionResult CensorshipManager()
        {
            return View();
        }
    }
}
