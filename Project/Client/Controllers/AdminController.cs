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

        /////////////////////////////////////////////////////
        //                                                 //
        //                     PRODUCT                     //
        //                                                 //
        /////////////////////////////////////////////////////


        // GetAll Product
        public async Task<IActionResult> ProductsManager()

        {
            List<ProductModel?> list = new();
            ResponseModel? response = await _productService.GetAllProductAsync();

            if (response != null && response.IsSuccess)
            {

                list = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(response.Result));

            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(list);
        }

        // UpdateProduct
        public async Task<IActionResult> UpdateProduct(string id)
        {
            ResponseModel? response = await _productService.GetProductByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                ResponseResultModel<UpdateProductModel>? data =
                    JsonConvert.DeserializeObject<ResponseResultModel<UpdateProductModel>>(Convert.ToString(response.Result));

                // Lấy dữ liệu từ trường "result" và gán vào model
                UpdateProductModel? model = data?.result;

                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(List<IFormFile> imageFiles, UpdateProductModel product, ScanFileRequest request)
        {
            // Chuyển đổi danh sách các danh mục từ product.Categories thành List<CategoryModel>
            var categories = product.Categories?.Select(c => new CategoryModel { CategoryName = c.CategoryName }).ToList();

            if (ModelState.IsValid)
            {
                // Cập nhật mô hình UpdateProductModel với các giá trị từ product
                var updateProduct = new UpdateProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Sold = product.Sold,
                    Discount = product.Discount,
                    Categories = categories,
                    Platform = product.Platform, // Giá trị enum PlatformType
                    Status = product.Status, // Giá trị enum ProductStatus
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    UserName = product.UserName
                };

                // Gửi yêu cầu cập nhật sản phẩm thông qua service
                var response = await _productService.UpdateProductAsync(imageFiles, updateProduct, request);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product updated successfully";
                    return RedirectToAction(nameof(ProductsManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

            // Nếu ModelState không hợp lệ, trả về lại model để hiển thị lỗi
            return View(product);
        }



        // DeleteProduct
        public async Task<IActionResult> DeleteProduct(string id)
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
        public async Task<IActionResult> DeleteProduct(ProductModel product)
        {
            // Thực hiện xóa sản phẩm
            ResponseModel? response = await _productService.DeleteProductAsysnc(product.Id);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully.";
                return RedirectToAction(nameof(ProductsManager));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }

        /*public async Task<IActionResult> Sort(string sort)
        {
            var response = await _productService.SortAsync(sort);
            if (response.IsSuccess)
            {
                return View("ProductList", response.Result);
            }
            ViewBag.ErrorMessage = response.Message;
            return View("Error");
        }*/

       /* public async Task<IActionResult> Filter(decimal? minrange, decimal? maxrange, int? sold, bool? discount, int? platform, string? category)
        {
            var response = await _productService.FilterAsync(minrange, maxrange, sold, discount, platform, category);
            if (response.IsSuccess)
            {
                return View("ProductList", response.Result);
            }
            ViewBag.ErrorMessage = response.Message;
            return View("Error");
        }  */ 
        // SearchProduct
        /*public async Task<IActionResult> Search(string keyword)
		{
			var products = await _productService.SearchProductAsync(keyword);
			return View("ProductsManager", products); // Hiển thị sản phẩm tìm được trong view ProductsManager
		}*/
        /*[HttpGet("SearchProducts")]
		public async Task<IActionResult> SearchProducts(string searchString)
		{
			if (string.IsNullOrWhiteSpace(searchString))
			{
				return View(new List<ProductModel>()); // Trả về view với danh sách rỗng nếu không có từ khóa tìm kiếm
			}

			var response = await _productService.SearchProductAsync(searchString);

			if (response.IsSuccess && response.Result is List<ProductModel> productList)
			{
				return View(productList); // Trả về view với kết quả tìm kiếm
			}

			TempData["ErrorMessage"] = response.Message; // Hiển thị thông báo lỗi
			return View(new List<ProductModel>());
		}*/

        /*[HttpPost]
        public async Task<IActionResult> ProductCreate(CreateProductModel createProduct)
        {
            if (ModelState.IsValid)
            {
                ResponseModel? response = await _productService.Crea(createProduct);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(ProductsManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                } 

            }
            return View(createProduct);
        }

        // Update Product ProductUpdate

       

        


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

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductModel product)
        {
            ResponseModel? response = await _productService.DeleteProductAsync(product.Id);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon deleted successfully";
                return RedirectToAction(nameof(ProductsManager));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }*/


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
