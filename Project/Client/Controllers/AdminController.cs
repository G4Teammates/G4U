using Client.Models;
using Client.Models.Product_Model;
using Client.Models.Product_Model.DTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.ProductInterface;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class AdminController (IUserService userService, IRepoProduct productService) : Controller
    {
	
		public readonly IUserService _userService = userService;
		public readonly IRepoProduct _productService = productService;
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
			List<UsersDTO?> list = new();
			ResponseModel? response = await _userService.GetAllUserAsync();

			if (response != null && response.IsSuccess)
			{

				list = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response.Result.ToString()));

			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(list);
		}


		[HttpPost]
		public async Task<IActionResult> UserCreate(UsersDTO user)
		{
			if (ModelState.IsValid)
			{
				ResponseModel? response = await _userService.CreateUserAsync(user);

				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Product created successfully";
					return RedirectToAction(nameof(UsersManager));
				}
				else
				{
					TempData["error"] = response?.Message;
				}

			}
			return View(user);
		}


        public async Task<IActionResult> UsersDelete(string id)
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

        public async Task<IActionResult> UsersDelete(UsersDTO user)
		{
			ResponseModel? response = await _userService.DeleteUser(user.Id);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "User deleted successfully";
				return RedirectToAction(nameof(UsersManager));
			}
			else
			{
				TempData["error"] = response?.Message;
			}
			return BadRequest();
		}



		/////////////////////////////////////////////////////////
		//                                                     //
		//                       PRODUCT                       //
		//                                                     //
		/////////////////////////////////////////////////////////
		
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


        // Create Product
        public async Task<IActionResult> ProductCreate()
        {
            // Optionally, you can fetch categories or other necessary data here
            ViewBag.Categories = await _productService.GetCategoriesAsync(); // Fetch categories for the view
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(CreateProductModel createProduct, List<IFormFile> imageFiles, IFormFile gameFile)
        {
            if (ModelState.IsValid)
            {
                // Prepare scan request for the game file
                var scanRequest = new ScanFileRequest
                {
                    gameFile = gameFile // Pass the game file from the form
                };

                // Assign image files and scan request to the CreateProductModel
                createProduct.ImageFiles = imageFiles;
                createProduct.Request = scanRequest;

                // Call the service to create the product
                ResponseModel? response = await _productService.CreateProductAsync(createProduct);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(ProductsManager)); // Redirect to the products list page
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

            // If ModelState is invalid, reload categories and return the view with errors
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(createProduct);
        }



        //DeleteProduct
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
        public async Task<IActionResult> ProductDelete(ProductModel deleteproduct)
        {
            ResponseModel? response = await _productService.DeleteProductAsync(deleteproduct.Id);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully";
                return RedirectToAction(nameof(UsersManager));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return BadRequest();
        }


        //UpdateProduct
        public async Task<IActionResult> ProductUpdate(string id)
        {
            ResponseModel? response = await _productService.GetProductByIdAsync(id);

            if (response != null && response.IsSuccess)
            {
                UpdateProductModel? model = JsonConvert.DeserializeObject<UpdateProductModel>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductUpdate(UpdateProductModel updateproduct)
        {
            if (ModelState.IsValid)
            {
                ResponseModel? response = await _productService.UpdateProductAsync(updateproduct);

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
            return View(updateproduct);
        }



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
