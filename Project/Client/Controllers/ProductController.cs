using Client.Models;
using Client.Models.CategorisDTO;
using Client.Models.ComentDTO;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Repositories.Interfaces.Comment;
using Client.Repositories.Interfaces.Product;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ocelot.Responses;

using System.IdentityModel.Tokens.Jwt;

namespace Client.Controllers
{
    public class ProductController(IHelperService helperService, IRepoProduct repoProduct, ICategoriesService categoryService, ICommentService commentService) : Controller
    {

        private readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoryService = categoryService;
        public readonly ICommentService _commentService = commentService;
        public async Task<IActionResult> ProductIndex()
        {
            return View();
        }
        public async Task<IActionResult> Product(int? page, int pageSize = 5)
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

        [HttpPost]
        public async Task<IActionResult> FilterProducts(
                                                        decimal? minRange,
                                                        decimal? maxRange,
                                                        int? sold,
                                                        bool? discount,
                                                        int? platform,
                                                        string category,
                                                        int? page,
                                                        int pageSize = 9999)
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

            return View("Product", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã lọc
        }

     
        public async Task<IActionResult> ProductDetail(string id)
        {
            ProductViewModel productViewModel = new ProductViewModel();
            ResponseModel? response = await _productService.GetDetailByIdAsync(id);
            ResponseModel? response1 = await _commentService.GetByproductId(id, 1, 9);

            if (response != null && response.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));

                // Deserialize response1.Result thành danh sách CommentDTOModel
                List<CommentDTOModel>? comments = JsonConvert.DeserializeObject<List<CommentDTOModel>>(Convert.ToString(response1.Result));

                if (model != null)
                {
                    // Gán model vào ProductViewModel
                    productViewModel.Product = new List<ProductModel> { model };

                    // Gán danh sách comments vào ProductViewModel
                    productViewModel.CommentDTOModels = comments ?? new List<CommentDTOModel>();
                }
            }
            else
            {
                TempData["error"] = response?.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return View(productViewModel);
        }





        public IActionResult Collection()
        {
            return View();
        }

    }
}
