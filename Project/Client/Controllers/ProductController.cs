using Azure;
using Client.Models;
using Client.Models.AuthenModel;
using Client.Models.CategorisDTO;
using Client.Models.ComentDTO;
using Client.Models.OrderModel;
using Client.Models.ProductDTO;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Categories;
using Client.Repositories.Interfaces.Comment;
using Client.Repositories.Interfaces.Order;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Client.Controllers
{
    public class ProductController(ICompositeViewEngine viewEngine ,IHelperService helperService, IRepoProduct repoProduct, ICategoriesService categoryService, ICommentService commentService, IUserService userService, IOrderService orderService) : Controller
    {

        private readonly IHelperService _helperService = helperService;
        public readonly IRepoProduct _productService = repoProduct;
        public readonly ICategoriesService _categoryService = categoryService;
        public readonly ICommentService _commentService = commentService;
        public readonly IUserService _userService = userService;
        private readonly ICompositeViewEngine _viewEngine = viewEngine;
        public readonly IOrderService _orderService = orderService;
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
                // Lấy tất cả danh mục sản phẩm
                ResponseModel? response1 = await _categoryService.GetAllCategoryAsync(1, 99);

                // Lấy dữ liệu sản phẩm của trang hiện tại
                ResponseModel? response = await _productService.GetAllProductAsync(pageNumber, pageSize);

                // Lấy tất cả sản phẩm để tính tổng số
                ResponseModel? response2 = await _productService.GetAllProductAsync(1, int.MaxValue);

                // Giải mã dữ liệu tổng số sản phẩm
                var total = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response2.Result.ToString()!));

                if (response != null && response.IsSuccess)
                {
                    // Dữ liệu của sản phẩm trong trang hiện tại
                    product.Product = JsonConvert.DeserializeObject<ICollection<ProductModel>>(Convert.ToString(response.Result.ToString()!));

                    // Danh mục sản phẩm
                    product.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response1.Result.ToString()!));

                    // Thông tin phân trang
                    product.pageNumber = pageNumber;
                    product.totalItem = total?.Count ?? 0; // Tổng số sản phẩm từ toàn bộ dữ liệu
                    product.pageSize = pageSize;
                    product.pageCount = (int)Math.Ceiling(product.totalItem / (double)pageSize);
                    ViewData["CurrentAction"] = "Product";
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
                    productViewModel.CategoriesModel = JsonConvert.DeserializeObject<ICollection<CategoriesModel>>(Convert.ToString(response3.Result.ToString()!));
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = pageNumber;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total.Count / (double)pageSize);
                    TempData["success"] = "Filter Products successfully";
                    ViewData["CurrentAction"] = "FilterProducts";
                    // Tạo đối tượng FilterParams để chứa các giá trị
                    var filterParams = new Dictionary<string, object>
                    {
                        { "minRange", minRange },
                        { "maxRange", maxRange },
                        { "sold", sold },
                        { "discount", discount },
                        { "category", category }
                    };
                    ViewData["Parameters"] = filterParams;
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

        //public async Task<IActionResult> ProductDetailLibrary(string id)
        //{
        //    IEnumerable<Claim> claim = HttpContext.User.Claims;
        //    ProductViewModel productViewModel = new ProductViewModel();

        //    ResponseModel? productReponse = await _productService.GetDetailByIdAsync(id);
        //    ResponseModel? cmtResponse = await _commentService.GetByproductId(id, 1, 9999);
        //    ResponseModel? productsReponse = await _productService.GetAllProductAsync(1, 99);


        //    if (productReponse != null && productReponse.IsSuccess)
        //    {

        //        ProductModel? ProductModel = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(productReponse.Result));

        //        List<ProductModel>? ProductsModel = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(productsReponse.Result));

        //        List<CommentDTOModel>? commentsModel = JsonConvert.DeserializeObject<List<CommentDTOModel>>(Convert.ToString(cmtResponse.Result));

        //        if (ProductModel != null)
        //        {
        //            // Gán model vào ProductViewModel
        //            productViewModel.Prod = new ProductModel
        //            {
        //                Name = ProductModel.Name,
        //                Price = ProductModel.Price,
        //                UserName = ProductModel.UserName,
        //                Description = ProductModel.Description,
        //                Categories = ProductModel.Categories,
        //                CreatedAt = ProductModel.CreatedAt,
        //                UpdatedAt = ProductModel.UpdatedAt,
        //                Platform = ProductModel.Platform,
        //                Interactions = ProductModel.Interactions,
        //                Links = ProductModel.Links,
        //                Sold = ProductModel.Sold,
        //                Status = ProductModel.Status,
        //                Id = ProductModel.Id,
        //                Discount = ProductModel.Discount,
        //                QrCode = ProductModel.QrCode
        //            };

        //            productViewModel.Product = ProductsModel ?? new List<ProductModel>();
        //            // Gán danh sách comments vào ProductViewModel
        //            productViewModel.CommentDTOModels = commentsModel ?? new List<CommentDTOModel>();

        //            productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
        //        }
        //    }
        //    else
        //    {
        //        TempData["error"] = productReponse?.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
        //        return NotFound();
        //    }

        //    // Trả về View với ProductViewModel
        //    return View(productViewModel);
        //}

        public async Task<IActionResult> ProductDetail(string id)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();

            ResponseModel? productReponse = await _productService.GetDetailByIdAsync(id);
            ResponseModel? cmtResponse = await _commentService.GetByproductId(id, 1, 9999);
            ResponseModel? productsReponse = await _productService.GetAllProductAsync(1,99);

            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            string i = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            ResponseModel? ItemResponse = await _orderService.GetItemsByCustomerId(i);

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

                if (ItemResponse != null && ItemResponse.IsSuccess)
                {
                    List<OrderItemModel>? orderProducts = JsonConvert.DeserializeObject<List<OrderItemModel>>(Convert.ToString(ItemResponse.Result));

                    ViewBag.HasOwned = false;
                    foreach (var item in orderProducts)
                    {
                        if (item.ProductId == id)
                        {
                            ViewBag.HasOwned = true;
                            List<LinkModel> urls = new List<LinkModel>();
                            foreach (var link in productViewModel.Prod.Links)
                            {
                                if (link.Url.Contains("drive.google.com"))
                                {
                                    urls.Add(link);
                                }
                            }
                            ViewBag.UrlsDownLoad = urls;
                            break;
                        }
                    }
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
        public async Task<IActionResult> SortProducts(string sort, int? page, int pageSize = 99)
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
                    ViewData["CurrentAction"] = "SortProducts";
                    ViewData["Parameters"] = sort;
                    ViewData["NamePara"] = "sort";
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
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            if (model.UserName == null)
            {
                TempData["error"] = "Please login first";
                return Json(new { success = false, message = TempData["error"] });
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                TempData["error"] = string.Join(", ", errors);
                return Json(new { success = false, message = TempData["error"] });
            }
            try
            {
                var userid = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
                // Gọi service CreateCommentAsync
                var response = await _commentService.CreateCommentAsync(model, userid);
                var modelCmt = JsonConvert.DeserializeObject<CommentDTOModel>(Convert.ToString(response.Result.ToString()!));
                
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Comment created successfully";

                    // Render PartialView to string
                    var html = await RenderViewAsync("_CommentPartial", modelCmt);

                    return Json(new { success = true, html = html , message = TempData["success"] });
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return Json(new { success = false, message = TempData["error"] });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return Json(new { success = false, message = TempData["error"] });
            }

        }
        [HttpPost]
        public async Task<IActionResult> IncreaseLike(string commentID)
        {
            try
            {
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                var userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                // Tạo model UserLikesModel và gán userName
                var userLikes = new Models.ComentDTO.UserLikesModel
                {
                    UserName = userName
                };
                if(userName != null)
                {
                    ResponseModel? response = await _commentService.IncreaseLike(commentID, userLikes);
                    if (response != null && response.IsSuccess)
                    {
                        return Json(new { success = true, commentId = commentID, newLikeCount = response.Result , message = response.Message });
                    }
                    else
                    {
                        return Json(new { success = false, message = response?.Message ?? "Đã xảy ra lỗi không xác định." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Please login first" });
                }
               
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DecreaseLike(string commentID)
        {
            try
            {
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                var userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                // Tạo model UserLikesModel và gán userName
                var userDisLikes = new Models.ComentDTO.UserDisLikesModel
                {
                    UserName = userName
                };
                if (userName != null)
                {
                    ResponseModel? response = await _commentService.DecreaseLike(commentID, userDisLikes);
                    if (response != null && response.IsSuccess)
                    {
                        return Json(new { success = true, commentId = commentID, newDislikeCount = response.Result , message = response.Message });
                    }
                    else
                    {
                        return Json(new { success = false, message = response?.Message ?? "Đã xảy ra lỗi không xác định." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Please login first" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }



        public async Task<IActionResult> Collection()
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();
            string un = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            string i = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;


            // Lấy các response từ các service
            ResponseModel? ProResponese = await _productService.GetAllProductsByUserName(un);
            ResponseModel? ItemResponse = await _orderService.GetItemsByCustomerId(i);
            ResponseModel? WishListResponse = await _userService.GetAllProductsInWishList(i);

            // Kiểm tra và gán oderitem nếu ItemResponse thành công
            if (ItemResponse != null && ItemResponse.IsSuccess)
            {
                productViewModel.oderitem = JsonConvert.DeserializeObject<List<OrderItemModel>>(Convert.ToString(ItemResponse.Result))
                    ?? new List<OrderItemModel>();
            }
            else
            {
                // Nếu không có ItemResponse hợp lệ, gán danh sách trống
                productViewModel.oderitem = new List<OrderItemModel>();
            }

            // Kiểm tra và gán Wishlist nếu WishListResponse thành công
            if (WishListResponse != null && WishListResponse.IsSuccess)
            {
                productViewModel.Wishlist = JsonConvert.DeserializeObject<List<WishlistModel>>(Convert.ToString(WishListResponse.Result))
                    ?? new List<WishlistModel>();
            }
            else
            {
                // Nếu không có WishListResponse hợp lệ, gán danh sách trống
                productViewModel.Wishlist = new List<WishlistModel>();
            }

            // Thêm các thông tin khác vào ProductViewModel
            productViewModel.userName = un;
            productViewModel.userID = i;

            // Trả về View với ProductViewModel
            return View(productViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> IncreaseLikeProduct(string productID)
        {
            try
            {
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                var userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                // Tạo model UserLikesModel và gán userName
                var userLikes = new Models.ProductDTO.UserLikesModel
                {
                    UserName = userName
                };
                if (userName != null)
                {
                    ResponseModel? response = await _productService.IncreaseLike(productID, userLikes);
                    if (response != null && response.IsSuccess)
                    {
                        return Json(new { success = true, productId = productID, newLikeCount = response.Result, message = response.Message });
                    }
                    else
                    {
                        return Json(new { success = false, message = response?.Message ?? "Đã xảy ra lỗi không xác định." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Please login first" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DecreaseLikeProduct(string productID)
        {
            try
            {
                IEnumerable<Claim> claim = HttpContext.User.Claims;
                var userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                // Tạo model UserLikesModel và gán userName
                var userDisLikes = new Models.ProductDTO.UserDisLikesModel
                {
                    UserName = userName
                };
                if (userName != null)
                {
                    ResponseModel? response = await _productService.DecreaseLike(productID, userDisLikes);
                    if (response != null && response.IsSuccess)
                    {
                        return Json(new { success = true, productId = productID, newDislikeCount = response.Result, message = response.Message });
                    }
                    else
                    {
                        return Json(new { success = false, message = response?.Message ?? "Đã xảy ra lỗi không xác định." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Please login first" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
            // Phương thức hỗ trợ để render PartialView thành chuỗi HTML
        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                // Thay đổi này sẽ tránh việc render layout khi gọi PartialView
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: false); // Đặt là false để không dùng layout

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                // Render view mà không có layout
                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public async Task<IActionResult> ViewAll(string viewString)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            ProductViewModel productViewModel = new ProductViewModel();

            ResponseModel? productReponse = await _productService.ViewMore(viewString);


            if (productReponse != null && productReponse.IsSuccess)
            {
                List<ProductModel>? ProductsModel = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(productReponse.Result));
                var total = ProductsModel.Count;
                int pageSize = 5;
                if (ProductsModel != null)
                {
                    

                    productViewModel.Product = ProductsModel ?? new List<ProductModel>();
                    // Gán danh sách comments vào ProductViewModel
                   
                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                    var data = productViewModel.Product;
                    productViewModel.pageNumber = 1;
                    productViewModel.totalItem = data.Count;
                    productViewModel.pageSize = pageSize;
                    productViewModel.pageCount = (int)Math.Ceiling(total / (double)pageSize);
                    TempData["success"] = "Sort Products successfully";
                }
            }
            else
            {
                TempData["error"] = productReponse?.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

            // Trả về View với ProductViewModel
            return View("Product",productViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SearchProduct(string searchString, int? page, int pageSize = 99)
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
                    TempData["success"] = "Search Products successfully";
                    ViewData["Parameters"] = searchString;
                    ViewData["NamePara"] = "searchString";
                    ViewData["CurrentAction"] = "SearchProduct";
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

            return View("~/Views/Product/Product.cshtml", productViewModel);
            // Trả về view ProductsManager với danh sách sản phẩm đã tìm kiếm
        }

    }
}
