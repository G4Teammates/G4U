using Client.Models;
using Client.Models.Product_Model;
using Client.Models.Product_Model.DTO;
using Client.Repositories.Interfaces.ProductInterface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class AdminController : Controller
    {
        public readonly IRepoProduct _productService;
        public AdminController(IRepoProduct productService)
        {
            _productService = productService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult UsersManager()
        {
            return View();
        }

        ////////////////////////////////////////////////////////////
        //                                                        //
        //                        PRODUCT                         //
        //                                                        //
        ////////////////////////////////////////////////////////////

        public async Task<IActionResult> ProductsManager()
		{
            List<ProductModel> list = new();
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
        /*[HttpPost]
        public async Task<IActionResult> ProductCreate(CreateProductModel model)
        {
            if (ModelState.IsValid)
            {
                ResponseModel? response = await _productService.CreateProductAsync(
                    model.Name,
                    model.Description,
                    model.Price,
                    model.Discount,
                    model.Categories,
                    model.Platform,
                    model.Status,
                    model.ImageFiles,
                    model.Request);

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

            // If the model state is not valid or product creation fails, return the view with the model
            return View(model);
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
