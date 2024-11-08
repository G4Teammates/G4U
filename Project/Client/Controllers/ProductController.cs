using Azure;
using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.CategorisDTO;
using Client.Models.ComentDTO;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Repositories.Interfaces.Comment;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Client.Controllers
{
    public class ProductController(IHelperService helperService, IRepoProduct repoProduct, ICategoriesService categoryService, ICommentService commentService, IUserService userService) : Controller
    {

        private readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoryService = categoryService;
        public readonly ICommentService _commentService = commentService;
        public readonly IUserService _userService = userService;
        public async Task<IActionResult> ProductIndex()
        {
            return View();
        }
        public async Task<IActionResult> Product(int? page, int pageSize = 99)
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
                    TempData["success"] = "Filter Products successfully";
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
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();

            ResponseModel? productReponse = await _productService.GetDetailByIdAsync(id);
            ResponseModel? cmtResponse = await _commentService.GetByproductId(id, 1, 9999);
            ResponseModel? productsReponse = await _productService.GetAllProductAsync(1,99);


            if (productReponse != null && productReponse.IsSuccess)
            {
                
                ProductModel? ProductModel = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(productReponse.Result));

                List<ProductModel>? ProductsModel = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(productsReponse.Result));
                
                List<CommentDTOModel>? commentsModel = JsonConvert.DeserializeObject<List<CommentDTOModel>>(Convert.ToString(cmtResponse.Result));

                if (ProductModel != null)
                {
                    // Gán model vào ProductViewModel
                    productViewModel.Prod = new ProductModel {Name = ProductModel.Name,
                                                                Price = ProductModel.Price,
                                                                UserName = ProductModel.UserName,
                                                                Description = ProductModel.Description,
                                                                Categories = ProductModel.Categories,
                                                                CreatedAt = ProductModel.CreatedAt,
                                                                UpdatedAt = ProductModel.UpdatedAt,
                                                                Platform = ProductModel.Platform,
                                                                Interactions = ProductModel.Interactions,
                                                                Links = ProductModel.Links,
                                                                Sold = ProductModel.Sold,
                                                                Status = ProductModel.Status,
                                                                Id = ProductModel.Id,
                                                                Discount = ProductModel.Discount,
                                                                QrCode = ProductModel.QrCode };

                    productViewModel.Product = ProductsModel ?? new List<ProductModel>();
                    // Gán danh sách comments vào ProductViewModel
                    productViewModel.CommentDTOModels = commentsModel ?? new List<CommentDTOModel>();

                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                }
            }
            else
            {
                TempData["error"] = productReponse?.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return View(productViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetReply(string parentId)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();
            ResponseModel? response = await _commentService.GetByParentIdAsync(parentId,1,9999);
            if (response != null && response.IsSuccess)
            {
                List<CommentDTOModel>? comments = JsonConvert.DeserializeObject<List<CommentDTOModel>>(Convert.ToString(response.Result));

                if (comments != null)
                {
                    // Gán danh sách comments vào ProductViewModel
                    productViewModel.Reply = comments ?? new List<CommentDTOModel>();
                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                }
            }
            else
            {
                TempData["error"] = response?.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return PartialView("_CommentReplies", productViewModel);
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
                    TempData["success"] = "Sort Products successfully";
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

            return View("Product", productViewModel); // Trả về view ProductsManager với danh sách sản phẩm đã sắp xếp
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
                    return RedirectToAction("ProductDetail", new { id = model.ProductId });
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return RedirectToAction("ProductDetail", new { id = model.ProductId });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ProductDetail", new { id = model.ProductId });
            }

        }


        public async Task<IActionResult> Collection()
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            string i = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            ResponseModel? ProResponese = await _productService.GetAllProductsByUserName(un);
            ResponseModel? WishListResponse = await _userService.GetAllProductsInWishList(i);
            /*ResponseModel? response2 = await _userService.GetUserAsync(i);*/

            if (ProResponese != null && ProResponese.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                //ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
                List<ProductModel>? ListProduct = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(ProResponese.Result));
                /*List<UsersDTO>? model1 = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response1.Result));*/
                List<WishlistModel>? Wishlist = JsonConvert.DeserializeObject<List<WishlistModel>>(Convert.ToString(WishListResponse.Result));

                if (ListProduct != null)
                {

                    productViewModel.Product = ListProduct ?? new List<ProductModel>();
                    /*productViewModel.User = model1 ?? new List<UsersDTO>();*/
                    productViewModel.Wishlist = Wishlist ?? new List<WishlistModel>();
                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                    productViewModel.userID = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
                }
            }
            else
            {
                TempData["error"] = ProResponese?.Message + WishListResponse .Message?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return View(productViewModel);
        }
    }
}
