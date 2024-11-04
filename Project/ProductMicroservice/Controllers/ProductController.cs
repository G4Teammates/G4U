    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using ProductMicroservice.DBContexts;
    using ProductMicroservice.DBContexts.Entities;
    using ProductMicroservice.DBContexts.Enum;
    using ProductMicroservice.Models;
    using ProductMicroservice.Models.DTO;
    using ProductMicroservice.Repostories;
    using X.PagedList.Extensions;

namespace ProductMicroService.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class ProductController : ControllerBase
        {
            private ResponseDTO _responseDTO;
            private readonly IRepoProduct _repoProduct;
            public ProductController(IRepoProduct repoProduct)
            {
                _responseDTO = new ResponseDTO();
                _repoProduct = repoProduct;
            }

            [HttpPost]
            [RequestSizeLimit(60 * 1024 * 1024)] // 50MB
            [RequestFormLimits(MultipartBodyLengthLimit = 60 * 1024 * 1024)] // Đặt giới hạn cho form multipart
            public async Task<IActionResult> CreateProduct(
                                [FromForm] string name,
                                [FromForm] string description,
                                [FromForm] decimal price,
                                [FromForm] float discount,
                                [FromForm] List<string> categories,
                                [FromForm] int platform,
                                [FromForm] int status,
                                [FromForm] List<IFormFile> imageFiles,
                                [FromForm] ScanFileRequest request,
                                [FromForm] string username
                                )
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(new { Errors = errors });
                }
                try
                {
                    // Kiểm tra tệp hình ảnh
                    if (imageFiles == null || !imageFiles.Any())
                    {
                        return BadRequest("Image files cannot be null or empty.");
                    }

                    // Kiểm tra tệp game
                    if (request?.gameFile == null || request.gameFile.Length == 0)
                    {
                        return BadRequest("Game file cannot be null or empty.");
                    }

                    // Kiểm tra giá trị Platform
                    if (!Enum.IsDefined(typeof(PlatformType), platform))
                    {
                        return BadRequest("Invalid platform value.");
                    }

                    // Kiểm tra giá trị Status
                    if (!Enum.IsDefined(typeof(ProductStatus), status))
                    {
                        return BadRequest("Invalid status value.");
                    }

                    // Chuyển đổi danh sách chuỗi thành danh sách CategoryModel
                    var categoryModels = categories.Select(c => new CategoryModel { CategoryName = c }).ToList();
                    var gameFile = request.gameFile;

                    var product = new CreateProductModel
                    {
                        Name = name,
                        Description = description,
                        Price = price,
                        Discount = discount,
                        Categories = categoryModels,
                        Platform = (PlatformType)platform,
                        Status = (ProductStatus)status
                    };

                    var newProduct = await _repoProduct.Moderate(imageFiles, product, gameFile, username);
                    if (newProduct.IsSuccess)
                    {
                        _responseDTO.Result = newProduct.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = newProduct.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while creating the Product: " + ex.Message;
                    return StatusCode(500, _responseDTO);
                }
            }







            [HttpGet("{id?}")]
            public async Task<IActionResult> GetById([FromRoute] string id)
            {
                try
                {
                    var Products = await _repoProduct.GetById(id);
                    if (Products.IsSuccess)
                    {
                        _responseDTO.Result = Products.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Products.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while getting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }

            [HttpGet("detail={id}")]
            public async Task<IActionResult> GetDetail([FromRoute] string id)
            {
                try
                {
                    var Product = await _repoProduct.GetDetail(id);
                    if (Product.IsSuccess)
                    {
                        _responseDTO.Result = Product.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Product.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while getting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }


            [HttpGet]
            public async Task<IActionResult> GetAll( int? page, int pageSize)
            {
                try
                {
                    int pageNumber = (page ?? 1);
                    var Pros = await _repoProduct.GetAll(pageNumber,pageSize);
                    if (Pros.IsSuccess)
                    {
                        _responseDTO.Result = Pros.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Pros.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while getting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }



            [HttpDelete("{id?}")]
            public async Task<IActionResult> DeleteSanPham([FromRoute] string id)
            {
                try
                {
                    var Products =await _repoProduct.DeleteProduct(id);
                    if (Products.IsSuccess)
                    {
                        _responseDTO.Result = Products.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Products.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while Deletting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }

            [HttpPut]
            public async Task<IActionResult> UpdateProduct(
                                                        [FromForm] string id,
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
                                                        [FromForm] DateTime createdAt,
                                                        [FromForm] List<string>? links, // Thay đổi kiểu thành List<string>
                                                        [FromForm] List<IFormFile>? imageFiles,
                                                        [FromForm] ScanFileRequest? request,
                                                        [FromForm] string username)
            {
                try
                {
                    // Chuyển đổi danh sách chuỗi thành danh sách CategoryModel
                    var categoryModels = categories.Select(c => new CategoryModel { CategoryName = c }).ToList();
                    var gameFile = request?.gameFile;

                    // Tạo danh sách links mới từ các chuỗi JSON
                    var linkModels = links.Select(linkJson => JsonConvert.DeserializeObject<LinkModel>(linkJson)).ToList();

                    var product = new UpdateProductModel
                    {
                        Id = id,
                        Name = name,
                        Description = description,
                        Price = price,
                        Sold = sold,
                        Interactions = new InteractionModel { NumberOfLikes = numOfLike, NumberOfViews = numOfView },
                        Discount = discount,
                        Categories = categoryModels,
                        Platform = (PlatformType)platform,
                        Status = (ProductStatus)status,
                        CreatedAt = createdAt,
                        Links = linkModels, // Lưu links dưới dạng LinkModel
                        UserName = username
                    };

                    // Kiểm tra nếu không có tệp image nào được gửi
                    if (imageFiles == null || imageFiles.Count == 0)
                    {
                        // Thực hiện cập nhật sản phẩm mà không cần tệp
                        var newProductNoFiles = await _repoProduct.UpdateProduct(null, product, gameFile);
                        if (newProductNoFiles.IsSuccess)
                        {
                            _responseDTO.Result = newProductNoFiles.Result;
                            return Ok(_responseDTO);
                        }
                        _responseDTO.Message = newProductNoFiles.Message;
                        return BadRequest(_responseDTO.Message);
                    }
                    // Kiểm tra nếu không có tệp game nào được gửi
                    else if (gameFile == null)
                    {
                        // Thực hiện cập nhật sản phẩm mà không cần tệp
                        var newProductNoFilesGame = await _repoProduct.UpdateProduct(imageFiles, product, null);
                        if (newProductNoFilesGame.IsSuccess)
                        {
                            _responseDTO.Result = newProductNoFilesGame.Result;
                            return Ok(_responseDTO);
                        }
                        _responseDTO.Message = newProductNoFilesGame.Message;
                        return BadRequest(_responseDTO.Message);
                    }
                    // Kiểm tra nếu không có tệp game nào được gửi
                    else if (gameFile == null && imageFiles == null)
                    {
                        // Thực hiện cập nhật sản phẩm mà không cần tệp
                        var newProductNoFiles = await _repoProduct.UpdateProduct(null, product, null);
                        if (newProductNoFiles.IsSuccess)
                        {
                            _responseDTO.Result = newProductNoFiles.Result;
                            return Ok(_responseDTO);
                        }
                        _responseDTO.Message = newProductNoFiles.Message;
                        return BadRequest(_responseDTO.Message);
                    }

                    var newProduct = await _repoProduct.UpdateProduct(imageFiles, product, gameFile);
                    if (newProduct.IsSuccess)
                    {
                        _responseDTO.Result = newProduct.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = newProduct.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while updatting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }


            [HttpGet("sort={sort}")]
            public async Task<IActionResult> Sort([FromRoute] string sort, int? page,int pageSize)
            {
                try
                {
                    int pageNumber = (page ?? 1);
                    var Pros = await _repoProduct.Sort(sort,pageNumber, pageSize);
                    if (Pros.IsSuccess)
                    {
                        _responseDTO.Result = Pros.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Pros.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while sortting the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }

            [HttpGet("search={searchString}")]
            public async Task<IActionResult> Search([FromRoute] string searchString, int? page, int pageSize)
            {
                try
                {
                    int pageNumber = (page ?? 1);
                    var Pros = await _repoProduct.Search(searchString, pageNumber, pageSize);
                    if (Pros.IsSuccess)
                    {
                        _responseDTO.Result = Pros.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Pros.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while searching the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }
            [HttpGet("filter")]
		    public async Task<IActionResult> Filter([FromQuery] decimal? minrange, [FromQuery] decimal? maxrange, [FromQuery] int? sold, [FromQuery] bool? Discount, [FromQuery] int? Platform, [FromQuery] string? Category, int? page, int pageSize)
		    {
                try
                {
                    int pageNumber = (page ?? 1);
                    var Pros = await _repoProduct.Filter(minrange, maxrange, sold, Discount, Platform, Category, pageNumber, pageSize);
                    if (Pros.IsSuccess)
                    {
                        _responseDTO.Result = Pros.Result;
                        return Ok(_responseDTO);
                    }
                    _responseDTO.Message = Pros.Message;
                    return BadRequest(_responseDTO.Message);
                }
                catch (Exception ex)
                {
                    _responseDTO.IsSuccess = false;
                    _responseDTO.Message = "An error occurred while filtering the Products: " + ex.Message;
                    return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
                }
            }
	    }
    }
