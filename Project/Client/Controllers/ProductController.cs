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
                // Gọi service CreateCommentAsync
                var response = await _commentService.CreateCommentAsync(model);
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
            ResponseModel? ProResponese = await _productService.GetAllProductsByUserName(un);
            ResponseModel? ItemResponse = await _orderService.GetItemsByCustomerId(i);
            ResponseModel? WishListResponse = await _userService.GetAllProductsInWishList(i);
            /*ResponseModel? response2 = await _userService.GetUserAsync(i);*/

            if (ProResponese != null && ProResponese.IsSuccess)
            {
                // Deserialize vào lớp trung gian với kiểu ProductModel
                //ProductModel? updateProductModel = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
                List<ProductModel>? ListProduct = JsonConvert.DeserializeObject<List<ProductModel>>(Convert.ToString(ProResponese.Result));
                /*List<UsersDTO>? model1 = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response1.Result));*/
                List<OrderItemModel>? Item = JsonConvert.DeserializeObject<List<OrderItemModel>>(Convert.ToString(ItemResponse.Result));
                List<WishlistModel>? Wishlist = JsonConvert.DeserializeObject<List<WishlistModel>>(Convert.ToString(WishListResponse.Result));
                if (ListProduct != null)
                {

                    productViewModel.Product = ListProduct ?? new List<ProductModel>();
                    /*productViewModel.User = model1 ?? new List<UsersDTO>();*/
                    productViewModel.oderitem = Item ?? new List<OrderItemModel>();
                    productViewModel.Wishlist = Wishlist ?? new List<WishlistModel>();
                    productViewModel.userName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
                    productViewModel.userID = claim.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
                }
            }
            else
            {
                TempData["error"] = ProResponese?.Message + ItemResponse.Message + WishListResponse.Message ?? "Đã có lỗi xảy ra khi lấy thông tin sản phẩm.";
                return NotFound();
            }

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

    }
}
