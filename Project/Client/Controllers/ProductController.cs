using Client.Models;
using Client.Models.Product_Model;
using Client.Models.Product_Model.DTO;
using Client.Repositories.Interfaces.ProductInterface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ocelot.Responses;

namespace Client.Controllers
{
    public class ProductController : Controller
    {
        public readonly IRepoProduct _productService;
        public ProductController(IRepoProduct productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> ProductIndex()
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

        /*public async Task<IActionResult> ProductCreate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProductCreate(string name, string description, decimal price, float discount, List<string> categories, int platform, int status, List<IFormFile> imageFiles, ScanFileRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseModel? response = await _productService.CreateProductAsync(name, description, price, discount, categories, platform, status, imageFiles, request);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }

            }
            return View(name, description, price, discount, categories, platform, status, imageFiles, request);
        }*/
        public IActionResult Product()
        {
            return View();
        }

        public IActionResult ProductDetail()
        {
            return View();
        }

        public IActionResult Collection()
        {
            return View();
        }

    }
}
