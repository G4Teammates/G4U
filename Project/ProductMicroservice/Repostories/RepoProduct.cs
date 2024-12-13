using AutoMapper;
using Azure.AI.ContentSafety;
using Azure;
using ProductMicroservice.DBContexts;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models;
using ProductMicroservice.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductMicroservice.DBContexts.Enum;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System.Text;
using ProductMicroservice.Models.Drive;
using RestSharp;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using System.Linq; // Đảm bảo rằng không quên import namespace này
using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Models.Initialization;
using ProductMicroservice.Repostories.Helper;
using X.PagedList.Extensions;
using ProductMicroservice.Models.Message;
using ProductMicroservice.Repostories.Messages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Caching.Memory;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Diagnostics;
using RabbitMQ.Client;

namespace ProductMicroservice.Repostories
{
    public class RepoProduct : IRepoProduct
    {
        #region declaration and initialization
        private readonly ProductDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        private readonly IHelper _helper;
        private readonly IMessage _message;
        private readonly IMemoryCache _memoryCache;


        public RepoProduct(IConfiguration configuration, ProductDbContext db, IMapper mapper, IHelper helper, IMessage message, IMemoryCache memoryCache)
        {
            _message = message;
            _helper = helper;
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
            var account = new Account(initializationModel.cloudNameCloudinary, initializationModel.apiKeyCloudinary, initializationModel.apiSecretCloudinary);
            _cloudinary = new Cloudinary(account);
            _memoryCache = memoryCache;
        }
        #endregion


        public async Task<ResponseDTO> UpdateRangeSoldAsync(OrderItemsResponse model)
        {
            ResponseDTO response = new();
            try
            {
                var productIds = model.ProductSoldModels.Select(m => m.ProductId).ToList();
                var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

                foreach (var item in model.ProductSoldModels)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Sold += item.Quantity;
                    }
                }

                await _db.SaveChangesAsync();
                response.Message = "Update successfully";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateProduct(List<IFormFile>? imageFiles, UpdateProductModel Product, IFormFile? gameFiles)
        {
            ResponseDTO response = new();
            try
            {
                #region Kiểm tra số lượng hình ảnh
                if (imageFiles != null && (imageFiles.Count + Product.Links.Count > 7))
                {
                    response.IsSuccess = false;
                    response.Message = $"Maximum 7 image files.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra trùng lặp trong danh sách categories
                var duplicateCategories = Product.Categories
                    .GroupBy(c => c.CategoryName.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicateCategories.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"The following categories are duplicated: {string.Join(", ", duplicateCategories)}";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra xem tên product có trùng lặp trong cơ sở dữ liệu
                var existingProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == Product.Name.ToLower() && p.Id != Product.Id);
                if (existingProduct)
                {
                    response.IsSuccess = false;
                    response.Message = $"The product name '{Product.Name}' is already in use.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra category có tồn tại hay không
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                bool _isExist = false; // Khởi tạo biến _isExist
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessage(Product.Categories, "CheckExist", "CheckExistCategory_For_RreateProduct", "CheckExistCategory_For_RreateProduct", ExchangeType.Direct, true, false, false, false);
                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnCategoryResponseReceived += (response) =>
                    {
                        Console.WriteLine($"Received response: {response.CategoryName}, _IsExist: {response.IsExist}");
                        _isExist = response.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(_isExist);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist)
                    {
                        response.IsSuccess = false;
                        response.Message = "The Category is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm tra user có tồn tại hay không
                int maxRetryAttempts2 = 3; // Số lần thử lại tối đa
                int retryCount2 = 0; // Đếm số lần thử lại
                bool _isExist2 = false; // Khởi tạo biến _isExist
                bool isCompleted2 = false; // Biến đánh dấu hoàn thành

                while (retryCount2 < maxRetryAttempts2 && !isCompleted2)
                {
                    _message.SendingMessage(Product.UserName, "CheckExist", "CheckExistUserName_For_RreateProduct", "CheckExistUserName_For_RreateProduct", ExchangeType.Direct, true, false, false, false);

                    var tcs2 = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnUserResponseReceived += (response2) =>
                    {
                        Console.WriteLine($"_IsExist: {response2.IsExist}");
                        _isExist2 = response2.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs2.Task.IsCompleted)
                        {
                            tcs2.SetResult(_isExist2);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask2 = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask2 = await Task.WhenAny(tcs2.Task, timeoutTask2);

                    // Nếu timeout xảy ra
                    if (completedTask2 == timeoutTask2)
                    {
                        Console.WriteLine($"Attempt {retryCount2 + 1} failed due to timeout.");
                        retryCount2++; // Tăng số lần thử lại

                        if (retryCount2 >= maxRetryAttempts2)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted2 = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist2)
                    {
                        response.IsSuccess = false;
                        response.Message = "User is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm duyệt description, name
                // Kiểm duyệt nội dung
                if (!_helper.IsContentAppropriate(Product.Description) || !_helper.IsContentAppropriate(Product.Name))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
                {
                    #region Kiểm duyệt hình ảnh, quét virus và update product

                    var product = await _db.Products.FindAsync(Product.Id);
                    if (product != null)
                    {

                        // Nếu không có file nào từ client, cập nhật sản phẩm không kiểm duyệt hoặc thêm link mới
                        if ((imageFiles == null || imageFiles.Count == 0) && gameFiles == null)
                        {
                            var proNoFile = await _helper.UpdateProduct(Product);
                            response.Result = _mapper.Map<Products>(proNoFile);
                            return response;
                        }

                        // Khởi tạo ContentSafetyClient và danh sách link từ client
                        var client = new ContentSafetyClient(new Uri(initializationModel.EndpointContentSafety), new AzureKeyCredential(initializationModel.ApiKeyContentSafety));
                        var linkModel = new List<LinkModel>(Product.Links);
                        var listUnsafe = new List<bool>();

                        // Kiểm duyệt và upload mỗi hình ảnh từ imageFiles nếu có
                        if (imageFiles != null && imageFiles.Count > 0)
                        {
                            var imageTasks = imageFiles.Select(async imageFile =>
                            {
                                using var imageStream = new MemoryStream();
                                await imageFile.CopyToAsync(imageStream);
                                imageStream.Position = 0;

                                var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));
                                var request = new AnalyzeImageOptions(imageData);
                                try
                                {
                                    var responseAnalyze = await client.AnalyzeImageAsync(request);
                                    var result = responseAnalyze.Value;

                                    // Kiểm tra kết quả kiểm duyệt
                                    if (result.CategoriesAnalysis.Any(c => c.Severity > 4))
                                    {
                                        listUnsafe.Add(true);
                                        return; // Nếu hình ảnh không đạt, ngừng xử lý
                                    }

                                    // Hình ảnh đạt yêu cầu, upload lên Cloudinary và thêm link mới vào danh sách
                                    var imageLink = await _helper.UploadImageCloudinary(imageFile);
                                    linkModel.Add(new LinkModel
                                    {
                                        ProviderName = "Cloudinary",
                                        Url = imageLink,
                                        Type = LinkType.External,
                                        Status = LinkStatus.Active,
                                        Censorship = new CensorshipModel
                                        {
                                            ProviderName = "Azure Content Safety",
                                            Description = "Content Safety",
                                            Status = CensorshipStatus.Access
                                        }
                                    });
                                }
                                catch (RequestFailedException)
                                {
                                    listUnsafe.Add(true);
                                }
                            });
                            await Task.WhenAll(imageTasks);
                        }
                        // Kiểm duyệt và upload cho gameFile nếu có
                        if (gameFiles != null)
                        {
                            // Tìm link Google Drive cũ
                            var existingGoogleDriveLink = linkModel.FirstOrDefault(l => l.ProviderName == "Google Drive");

                            if (existingGoogleDriveLink != null)
                            {
                                // Xóa link cũ
                                linkModel.Remove(existingGoogleDriveLink);
                            }
                            string scan = await _helper.ScanFileForVirus(gameFiles);
                            if (scan != "OK")
                            {
                                listUnsafe.Add(true);
                            }
                            else
                            {
                                var gameLink = await _helper.UploadFileToGoogleDrive(gameFiles);
                                linkModel.Add(new LinkModel
                                {
                                    ProviderName = "Google Drive",
                                    Url = gameLink,
                                    Type = LinkType.External,
                                    Status = LinkStatus.Active,
                                    Censorship = new CensorshipModel
                                    {
                                        ProviderName = "VirusTotal",
                                        Description = "Scan Virus",
                                        Status = CensorshipStatus.Access
                                    }
                                });
                            }
                        }

                        // Kiểm tra nếu có file không hợp lệ
                        if (listUnsafe.Any())
                        {
                            response.IsSuccess = false;
                            response.Message = "Have any file not Safety";
                            return response; // Ngưng hàm và trả về response
                        }

                        // Cập nhật links của product với linkModel
                        Product.Links = linkModel;
                        var proHaveFile = await _helper.UpdateProduct(Product);
                        response.Result = _mapper.Map<Products>(proHaveFile);// Thực hiện cập nhật sản phẩm

                        var totalRequest = await TotalRequest();
                        _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Not found any Product";
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> GetAll(int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Pros = await _db.Products.ToListAsync();
                if (Pros != null)
                {
                    response.Result = _mapper.Map<ICollection<Products>>(Pros).ToPagedList(page, pageSize);
                    var totalRequest = await TotalRequest();
                    _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> GetById(string id)
        {
            ResponseDTO response = new();
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product != null)
                {
                    response.Result = _mapper.Map<Products>(product);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseDTO> GetDetail(string id)
        {
            ResponseDTO response = new();
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product != null)
                {
                    // Tạo key cache cho thời gian lượt xem cuối cùng của sản phẩm
                    string cacheKey = $"Product_{id}_LastViewedTime";

                    // Lấy thời gian lượt xem cuối cùng từ cache
                    DateTime? lastViewedTime = _memoryCache.Get<DateTime?>(cacheKey);

                    if (lastViewedTime == null || (DateTime.Now - lastViewedTime.Value).TotalSeconds >= 30)
                    {
                        // Nếu chưa đủ 30 giây, tăng lượt xem và cập nhật thời gian
                        product.Interactions.NumberOfViews++;

                        // Cập nhật thời gian lượt xem vào cache
                        _memoryCache.Set(cacheKey, DateTime.Now, TimeSpan.FromSeconds(30)); // Lưu trong cache 30 giây

                        // Cập nhật lại sản phẩm trong cơ sở dữ liệu
                        _db.Products.Update(product);
                        await _db.SaveChangesAsync();
                    }

                    response.Result = _mapper.Map<Products>(product);

                    var totalRequest = await TotalRequest();
                    _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> DeleteProduct(string id)
        {
            ResponseDTO response = new();
            try
            {
                var Product = await _db.Products.FindAsync(id);
                if (Product != null)
                {
                    _db.Products.Remove(Product);
                    await _db.SaveChangesAsync();

                    var totalRequest = await TotalRequest();
                    _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                    response.Message = "Delete successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Comment";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> Sort(string sort, int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Products = _db.Products.AsQueryable();  // Lấy dữ liệu từ cơ sở dữ liệu

                var productList = await Products.ToListAsync();  // Thực thi truy vấn và lấy toàn bộ danh sách sản phẩm

                // Sắp xếp trong bộ nhớ (sau khi tải dữ liệu)
                switch (sort)
                {
                    case "ascPrice":
                        productList = productList.OrderBy(x => x.Price).ToList();
                        break;
                    case "descPrice":
                        productList = productList.OrderByDescending(x => x.Price).ToList();
                        break;
                    case "ascView":
                        productList = productList.OrderBy(x => x.Interactions.NumberOfViews).ToList();
                        break;
                    case "descView":
                        productList = productList.OrderByDescending(x => x.Interactions.NumberOfViews).ToList();
                        break;
                    case "ascLike":
                        productList = productList.OrderBy(x => x.Interactions.NumberOfLikes).ToList();
                        break;
                    case "descLike":
                        productList = productList.OrderByDescending(x => x.Interactions.NumberOfLikes).ToList();
                        break;
                    case "ascSold":
                        productList = productList.OrderBy(x => x.Sold).ToList();
                        break;
                    case "descSold":
                        productList = productList.OrderByDescending(x => x.Sold).ToList();
                        break;
                    case "free":
                        productList = productList.Where(x => x.Price == 0).ToList();
                        break;
                    default:
                        break;
                }

                // Ánh xạ kết quả và phân trang
                response.Result = productList.ToPagedList(page, pageSize);

                if (response.Result == null)
                {
                    response.IsSuccess = false;
                    response.Message = "No products found.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }


        public async Task<ResponseDTO> Search(string searchstring, int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Pros = _db.Products.AsQueryable();
                if (!string.IsNullOrEmpty(searchstring))
                {
                    // Chuyển từ khóa tìm kiếm về dạng chữ thường
                    searchstring = searchstring.ToLower();

                    // Tìm kiếm theo name, category name và username
                    var result = Pros.Where(x =>
                        x.Name.ToLower().Contains(searchstring) || // Tìm trong tên sản phẩm
                        x.Categories.Any(c => c.CategoryName.ToLower().Contains(searchstring)) || // Tìm trong tên category
                        x.UserName.ToLower().Contains(searchstring)); // Tìm trong username

                    if (result.Any())
                    {
                        response.Result = _mapper
                            .Map<ICollection<Products>>(result)
                            .ToPagedList(page, pageSize); // Áp dụng phân trang
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "No matching products found.";
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Search string cannot be empty.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> Filter(decimal? minrange, decimal? maxrange, int? sold, bool? Discount, int? Platform, string? Category, int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                if(maxrange < minrange)
                {
                    response.IsSuccess = false;
                    response.Message = "The maxrange should not be less than the minrange ...";
                    return response;
                }
                // Bắt đầu với tất cả các sản phẩm
                var query = _db.Products.AsQueryable();
                var productList = await query.ToListAsync();  // Thực thi truy vấn và lấy toàn bộ danh sách sản phẩm
                if (productList != null)
                {
                    /*response.Result = _mapper.Map<ICollection<Products>>(Pros).ToPagedList(page, pageSize);*/
                    // Lọc theo khoảng giá
                    if (minrange.HasValue && maxrange.HasValue)
                    {
                        productList = productList.Where(p => p.Price >= minrange.Value && p.Price <= maxrange.Value).ToList();
                    }
                    else if (minrange.HasValue)
                    {
                        productList = productList.Where(p => p.Price >= minrange.Value).ToList();
                    }
                    else if (maxrange.HasValue)
                    {
                        productList = productList.Where(p => p.Price <= maxrange.Value).ToList();
                    }

                    // Lọc theo số lượng đã bán
                    if (sold.HasValue)
                    {
                        productList = productList.Where(p => p.Sold >= sold.Value).ToList();
                    }

                    // Lọc theo giảm giá
                    if (Discount.HasValue)
                    {
                        if (Discount.Value)
                        {
                            productList = productList.Where(p => p.Discount > 0).ToList(); // Lọc các sản phẩm có giảm giá
                        }
                        else
                        {
                            productList = productList.Where(p => p.Discount == 0).ToList(); // Lọc các sản phẩm không giảm giá
                        }
                    }

                    // Lọc theo nền tảng
                    if (Platform.HasValue)
                    {
                        productList = productList.Where(p => (int)p.Platform == Platform.Value).ToList(); // so sánh với enum PlatformType
                    }

                    // Lọc theo category
                    if (!string.IsNullOrEmpty(Category))
                    {
                        productList = productList.Where(p => p.Categories.Any(c => c.CategoryName.Equals(Category, StringComparison.OrdinalIgnoreCase))).ToList();
                    }

                    // Kiểm tra nếu không có sản phẩm nào sau khi lọc
                    if (!productList.Any())
                    {
                        response.IsSuccess = false;
                        response.Message = "Not found any Product";
                        return response; // Kết thúc tại đây nếu không có sản phẩm nào
                    }

                    // Phân trang và trả về kết quả
                    response.Result = _mapper.Map<ICollection<Products>>(productList.ToList()).ToPagedList(page, pageSize);
                    response.IsSuccess = true; // Đánh dấu là thành công
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> Moderate(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username)
        {
            ResponseDTO response = new();
            try
            {
                #region Kiểm tra số ảnh tối đa
                if (imageFiles.Count > 7)
                {
                    response.IsSuccess = false;
                    response.Message = $"Maximum 7 image files.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra trùng lặp trong danh sách categories
                var duplicateCategories = Product.Categories
                    .GroupBy(c => c.CategoryName.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicateCategories.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"The following categories are duplicated: {string.Join(", ", duplicateCategories)}";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra xem tên product có trùng lặp trong cơ sở dữ liệu
                var existingProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == Product.Name.ToLower());
                if (existingProduct)
                {
                    response.IsSuccess = false;
                    response.Message = $"The product name '{Product.Name}' is already in use.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra category có tồn tại hay không
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                bool _isExist = false; // Khởi tạo biến _isExist
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessage(Product.Categories, "CheckExist", "CheckExistCategory_For_RreateProduct", "CheckExistCategory_For_RreateProduct", ExchangeType.Direct, true, false, false, false);

                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnCategoryResponseReceived += (response) =>
                    {
                        Console.WriteLine($"Received response: {response.CategoryName}, _IsExist: {response.IsExist}");
                        _isExist = response.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(_isExist);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist)
                    {
                        response.IsSuccess = false;
                        response.Message = "The Category is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm tra user có tồn tại hay không
                int maxRetryAttempts2 = 3; // Số lần thử lại tối đa
                int retryCount2 = 0; // Đếm số lần thử lại
                bool _isExist2 = false; // Khởi tạo biến _isExist
                bool isCompleted2 = false; // Biến đánh dấu hoàn thành

                while (retryCount2 < maxRetryAttempts2 && !isCompleted2)
                {
                    _message.SendingMessage(username, "CheckExist", "CheckExistUserName_For_RreateProduct", "CheckExistUserName_For_RreateProduct", ExchangeType.Direct, true, false, false, false);

                    var tcs2 = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnUserResponseReceived += (response2) =>
                    {
                        Console.WriteLine($"_IsExist: {response2.IsExist}");
                        _isExist2 = response2.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs2.Task.IsCompleted)
                        {
                            tcs2.SetResult(_isExist2);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask2 = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask2 = await Task.WhenAny(tcs2.Task, timeoutTask2);

                    // Nếu timeout xảy ra
                    if (completedTask2 == timeoutTask2)
                    {
                        Console.WriteLine($"Attempt {retryCount2 + 1} failed due to timeout.");
                        retryCount2++; // Tăng số lần thử lại

                        if (retryCount2 >= maxRetryAttempts2)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted2 = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist2)
                    {
                        response.IsSuccess = false;
                        response.Message = "User is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm duyệt description, name
                // Kiểm duyệt nội dung
                if (!_helper.IsContentAppropriate(Product.Description) || !_helper.IsContentAppropriate(Product.Name))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
                {
                    #region Kiểm duyệt hình ảnh, quét virus và add product
                    if (imageFiles != null || imageFiles.Count != 0)
                    {
                        /*response.Result = _mapper.Map<Products>(product);*/

                        Products productEntity = null;
                        var client = new ContentSafetyClient(new Uri(initializationModel.EndpointContentSafety), new AzureKeyCredential(initializationModel.ApiKeyContentSafety));
                        var linkModel = new List<LinkModel>();
                        var listUnsafe = new List<bool>();

                        var imageTasks = imageFiles.Select(async imageFile =>
                        {
                            using var imageStream = new MemoryStream();
                            await imageFile.CopyToAsync(imageStream);
                            imageStream.Position = 0;

                            var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));
                            var request = new AnalyzeImageOptions(imageData);
                            try
                            {
                                var response = await client.AnalyzeImageAsync(request);
                                var result = response.Value;

                                // Kiểm tra kết quả kiểm duyệt
                                if (result.CategoriesAnalysis.Any(c => c.Severity > 4))
                                {
                                    listUnsafe.Add(true);
                                    return; // Nếu hình ảnh không đạt, ngừng xử lý
                                }

                                // Hình ảnh đạt yêu cầu, upload lên Cloudinary và thêm link mới vào danh sách
                                var imageLink = await _helper.UploadImageCloudinary(imageFile);
                                linkModel.Add(new LinkModel
                                {
                                    ProviderName = "Cloudinary",
                                    Url = imageLink,
                                    Type = LinkType.External,
                                    Status = LinkStatus.Active,
                                    Censorship = new CensorshipModel
                                    {
                                        ProviderName = "Azure Content Safety",
                                        Description = "Content Safety",
                                        Status = CensorshipStatus.Access
                                    }
                                });
                            }
                            catch (RequestFailedException)
                            {
                                listUnsafe.Add(true);
                            }
                        });
                        string scan = await _helper.ScanFileForVirus(gameFiles);
                        if (scan != "OK")
                        {
                            listUnsafe.Add(true);
                        }
                        else
                        {
                            var gameLink = await _helper.UploadFileToGoogleDrive(gameFiles);
                            linkModel.Add(new LinkModel
                            {
                                ProviderName = "Google Drive",
                                Url = gameLink,
                                Type = LinkType.External,
                                Status = LinkStatus.Active,
                                Censorship = new CensorshipModel
                                {
                                    ProviderName = "VirusTotal",
                                    Description = "Scan Virus",
                                    Status = CensorshipStatus.Access
                                }
                            });
                        }
                        await Task.WhenAll(imageTasks);
                        // Kiểm tra nếu có file không hợp lệ
                        if (listUnsafe.Any())
                        {
                            response.IsSuccess = false;
                            response.Message = "Have any file not Safety";
                            return response; // Ngưng hàm và trả về response
                        }

                        productEntity = await _helper.CreateProduct(Product, linkModel, username);
                        response.Result = _mapper.Map<Products>(productEntity);// Thực hiện cập nhật sản phẩm

                        var totalRequest = await TotalRequest();
                        _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Not found any File to create Product";
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<List<Products>> GetProductsByCategoryNameAsync(string categoryName)
        {
            return await _db.Products
                .Where(x => x.Categories.Any(c => c.CategoryName.Contains(categoryName)))
                .ToListAsync();
        }


        public async Task<ResponseDTO> TotalRequest()
        {
            ResponseDTO response = new();
            try
            {

                var Pros = await _db.Products.ToListAsync();
                if (Pros != null)
                {
                    var totalPro = Pros.Count;
                    var totalSold = Pros.Sum(pro => pro.Sold);
                    var totalView = Pros.Sum(pro => pro.Interactions.NumberOfViews);
                    var totalRequest = new TotalRequest()
                    {
                        totalProducts = totalPro,
                        totalSolds = totalSold,
                        totalViews = totalView,
                        updateAt = DateTime.Now,
                    };
                    response.Result = totalRequest;
                    return response;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> GetAllProductsByUserName(string userName)
        {
            ResponseDTO response = new();
            try
            {
                var products = await _db.Products.Where(p => p.UserName == userName).ToListAsync();

                if (products.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "This user doesn't have any product";
                    return response;
                }
                response.Message = "Get All Products Success";
                response.Result = _mapper.Map<ICollection<Products>>(products);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> IncreaseLike(string productId, UserLikesModel userLike)
        {
            ResponseDTO response = new();
            var product = await _db.Products.FindAsync(productId);
            var checkExitUser = product?.Interactions.UserLikes.FirstOrDefault(ul => ul.UserName == userLike.UserName);
            if (checkExitUser == null)
            {
                try
                {
                    if (product != null)
                    {
                        product.Interactions.NumberOfLikes++;
                        product.Interactions.UserLikes.Add(_mapper.Map<UserLikes>(userLike));
                        _db.Products.Update(product);
                        await _db.SaveChangesAsync();

                        response.IsSuccess = true;
                        response.Result = product.Interactions.NumberOfLikes;
                        response.Message = "Increased like count successfully.";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "product not found.";
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }
                return response;
            }
            else
            {
                response.IsSuccess = true;
                response.Result = product.Interactions.NumberOfLikes;
                response.Message = "You liked this product before.";
                return response;
            }
        }

        public async Task<ResponseDTO> DecreaseLike(string productId, UserDisLikesModel userDisLike)
        {
            ResponseDTO response = new();
            var product = await _db.Products.FindAsync(productId);
            var checkExitUser = product?.Interactions.UserDisLikes.FirstOrDefault(ul => ul.UserName == userDisLike.UserName);
            if (checkExitUser == null)
            {
                try
                {
                    if (product != null)
                    {
                        // Tăng số lượng dislike thay vì giảm số lượng like
                        product.Interactions.NumberOfDisLikes++;
                        product.Interactions.UserDisLikes.Add(_mapper.Map<UserDisLikes>(userDisLike));
                        _db.Products.Update(product);
                        await _db.SaveChangesAsync();

                        response.IsSuccess = true;
                        response.Result = product.Interactions.NumberOfDisLikes;
                        response.Message = "Increased dislike count successfully.";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "product not found.";
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }
                return response;
            }
            else
            {
                response.IsSuccess = true;
                response.Result = product.Interactions.NumberOfDisLikes;
                response.Message = "You disliked this product before.";
                return response;
            }

        }

        public async Task<ResponseDTO> ViewMore(string viewString)
        {
            ResponseDTO response = new();
            try
            {
                List<Products> products;

                var listPro = await _db.Products.Where(p => p.Categories.Any(c => c.CategoryName.Contains(viewString))).ToListAsync();
                if (listPro != null && listPro.Count != 0)
                {
                    products = listPro;
                    response.IsSuccess = true;
                    response.Result = products;
                    return response;
                }
                switch (viewString.ToLower())
                {
                    case "discount":
                        // Lấy ra các sản phẩm có discount khác 0 và sắp xếp giảm dần theo discount
                        products = await _db.Products
                            .Where(p => p.Discount != 0)
                            .OrderByDescending(p => p.Discount)
                            .ToListAsync();
                        break;

                    case "popular":
                        // Lấy ra các sản phẩm có numOfViews khác 0 và sắp xếp giảm dần theo numOfViews
                        products = await _db.Products
                            .Where(p => p.Interactions.NumberOfViews != 0)
                            .OrderByDescending(p => p.Interactions.NumberOfViews)
                            .ToListAsync();
                        break;

                    case "free":
                        // Lấy ra các sản phẩm có price bằng 0
                        products = await _db.Products
                            .Where(p => p.Price == 0)
                            .ToListAsync();
                        break;

                    case "new":
                        // Lấy ra các sản phẩm được tạo trong tháng hiện tại và sắp xếp theo ngày tạo (mới nhất trước)
                        var currentMonth = DateTime.Now.Month;
                        var currentYear = DateTime.Now.Year;
                        products = await _db.Products
                            .Where(p => p.CreatedAt.Month == currentMonth && p.CreatedAt.Year == currentYear)
                            .OrderByDescending(p => p.CreatedAt)
                            .ToListAsync();
                        break;
                    default:

                        // Nếu không khớp với bất kỳ điều kiện nào, trả về thông báo không tìm thấy
                        response.IsSuccess = false;
                        response.Message = "Invalid view type";
                        return response;
                }

                if (products != null && products.Any())
                {
                    response.Result = products;
                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "No products found based on the filter criteria";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ProductGroupByUserData> Data(TotalGroupByUserResponse response)
        {
            var result = new ProductGroupByUserData();

            /*// Đảm bảo response.CreateAt là UTC và có thời gian là 00:00:00
            var startOfDayUtc = DateTime.SpecifyKind(response.CreateAt.Date, DateTimeKind.Utc);

            // Query the products where UserName matches and CreateAt is less than or equal to the provided date
            var products = await _db.Products.Where(p => p.UserName == response.UserName && p.CreatedAt <= startOfDayUtc).ToListAsync();*/


            // Lấy tháng và năm từ response.CreateAt
            var targetMonth = response.CreateAt.Month;
            var targetYear = response.CreateAt.Year;

            // Query các sản phẩm có UserName trùng và tháng, năm của CreatedAt trùng với tháng, năm được truyền vào
            var products = await _db.Products
                .Where(p => p.UserName == response.UserName &&
                            p.CreatedAt.Month == targetMonth &&
                            p.CreatedAt.Year == targetYear)
                .ToListAsync();

            // Tính toán các giá trị cần thiết từ danh sách sản phẩm
            result.Views = products.Sum(p => p.Interactions?.NumberOfViews ?? 0);  // Tổng số lượt xem
            result.Products = products.Count();  // Số lượng sản phẩm
            result.Solds = products.Sum(p => p.Sold);  // Tổng số sản phẩm đã bán

            return result;
        }

        public async Task<ResponseDTO> GetProductByStatus(int Status)
        {
            ResponseDTO response = new();
            try
            {
                var products = await _db.Products.Where(p => p.Status == (ProductStatus)Status).ToListAsync();

                if (products.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "This Status doesn't have any product";
                    return response;
                }

                response.Message = "Get Products Success";
                response.Result = _mapper.Map<ICollection<Products>>(products);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseDTO> GetProductByPlatform(int Platform)
        {
            ResponseDTO response = new();
            try
            {
                var products = await _db.Products.Where(p => p.Platform == (PlatformType)Platform).ToListAsync();

                if (products.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "This Platform doesn't have any product";
                    return response;
                }

                response.Message = "Get Products Success";
                response.Result = _mapper.Map<ICollection<Products>>(products);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> ModerateClone(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username)
        {
            ResponseDTO response = new();
            try
            {
                #region Kiểm tra số ảnh tối đa
                if (imageFiles.Count > 7)
                {
                    response.IsSuccess = false;
                    response.Message = $"Maximum 7 image files.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra trùng lặp trong danh sách categories
                var duplicateCategories = Product.Categories
                    .GroupBy(c => c.CategoryName.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicateCategories.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"The following categories are duplicated: {string.Join(", ", duplicateCategories)}";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra xem tên product có trùng lặp trong cơ sở dữ liệu
                var existingProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == Product.Name.ToLower());
                if (existingProduct)
                {
                    response.IsSuccess = false;
                    response.Message = $"The product name '{Product.Name}' is already in use.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra category có tồn tại hay không
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                bool _isExist = false; // Khởi tạo biến _isExist
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessage(Product.Categories, "CheckExist", "CheckExistCategory_For_RreateProduct", "CheckExistCategory_For_RreateProduct", ExchangeType.Direct, true, false, false, false);

                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnCategoryResponseReceived += (response) =>
                    {
                        Console.WriteLine($"Received response: {response.CategoryName}, _IsExist: {response.IsExist}");
                        _isExist = response.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(_isExist);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist)
                    {
                        response.IsSuccess = false;
                        response.Message = "The Category is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm tra user có tồn tại hay không
                int maxRetryAttempts2 = 3; // Số lần thử lại tối đa
                int retryCount2 = 0; // Đếm số lần thử lại
                bool _isExist2 = false; // Khởi tạo biến _isExist
                bool isCompleted2 = false; // Biến đánh dấu hoàn thành

                while (retryCount2 < maxRetryAttempts2 && !isCompleted2)
                {
                    _message.SendingMessage(username, "CheckExist", "CheckExistUserName_For_RreateProduct", "CheckExistUserName_For_RreateProduct", ExchangeType.Direct, true, false, false, false);

                    var tcs2 = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnUserResponseReceived += (response2) =>
                    {
                        Console.WriteLine($"_IsExist: {response2.IsExist}");
                        _isExist2 = response2.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs2.Task.IsCompleted)
                        {
                            tcs2.SetResult(_isExist2);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask2 = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask2 = await Task.WhenAny(tcs2.Task, timeoutTask2);

                    // Nếu timeout xảy ra
                    if (completedTask2 == timeoutTask2)
                    {
                        Console.WriteLine($"Attempt {retryCount2 + 1} failed due to timeout.");
                        retryCount2++; // Tăng số lần thử lại

                        if (retryCount2 >= maxRetryAttempts2)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted2 = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist2)
                    {
                        response.IsSuccess = false;
                        response.Message = "User is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm duyệt description, name
                // Kiểm duyệt nội dung
                if (!_helper.IsContentAppropriate(Product.Description) || !_helper.IsContentAppropriate(Product.Name))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
                {
                    #region Kiểm duyệt hình ảnh, quét virus và add product
                    if (imageFiles != null || imageFiles.Count != 0)
                    {
                        /*response.Result = _mapper.Map<Products>(product);*/

                        Products productEntity = null;
                        var client = new ContentSafetyClient(new Uri(initializationModel.EndpointContentSafety), new AzureKeyCredential(initializationModel.ApiKeyContentSafety));
                        var linkModel = new List<LinkModel>();
                        var listUnsafe = new List<bool>();

                        var imageTasks = imageFiles.Select(async imageFile =>
                        {
                            using var imageStream = new MemoryStream();
                            await imageFile.CopyToAsync(imageStream);
                            imageStream.Position = 0;

                            var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));
                            var request = new AnalyzeImageOptions(imageData);
                            try
                            {
                                var response = await client.AnalyzeImageAsync(request);
                                var result = response.Value;

                                // Kiểm tra kết quả kiểm duyệt
                                if (result.CategoriesAnalysis.Any(c => c.Severity > 4))
                                {
                                    listUnsafe.Add(true);
                                    return; // Nếu hình ảnh không đạt, ngừng xử lý
                                }

                                // Hình ảnh đạt yêu cầu, upload lên Cloudinary và thêm link mới vào danh sách
                                var imageLink = await _helper.UploadImageCloudinary(imageFile);
                                linkModel.Add(new LinkModel
                                {
                                    ProviderName = "Cloudinary",
                                    Url = imageLink,
                                    Type = LinkType.External,
                                    Status = LinkStatus.Active,
                                    Censorship = new CensorshipModel
                                    {
                                        ProviderName = "Azure Content Safety",
                                        Description = "Content Safety",
                                        Status = CensorshipStatus.Access
                                    }
                                });
                            }
                            catch (RequestFailedException)
                            {
                                listUnsafe.Add(true);
                            }
                        });

                        // Check rar password của file game
                        if (!IsCorrectPassword(gameFiles, Product.WinrarPassword))
                        {
                            throw new Exception("Rar password is incorrect please enter the correct password");
                        }

                        string scan = await _helper.ScanFileForVirus(gameFiles);
                        if (scan != "OK")
                        {
                            listUnsafe.Add(true);
                        }
                        else
                        {
                            var gameLink = await _helper.UploadFileToGoogleDrive(gameFiles);
                            linkModel.Add(new LinkModel
                            {
                                ProviderName = "Google Drive",
                                Url = gameLink,
                                Type = LinkType.External,
                                Status = LinkStatus.Active,
                                Censorship = new CensorshipModel
                                {
                                    ProviderName = "VirusTotal",
                                    Description = "Scan Virus",
                                    Status = CensorshipStatus.Access
                                }
                            });
                        }
                        await Task.WhenAll(imageTasks);
                        // Kiểm tra nếu có file không hợp lệ
                        if (listUnsafe.Any())
                        {
                            response.IsSuccess = false;
                            response.Message = "Have any file not Safety";
                            return response; // Ngưng hàm và trả về response
                        }

                        productEntity = await _helper.CreateProductClone(Product, linkModel, username);
                        response.Result = _mapper.Map<Products>(productEntity);// Thực hiện cập nhật sản phẩm

                        var totalRequest = await TotalRequest();
                        _message.SendingMessage(totalRequest.Result, "Stastistical", "totalSold_totalProduct_for_stastistical", "totalSold_totalProduct_for_stastistical", ExchangeType.Direct, true, false, false, false);
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Not found any File to create Product";
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> UpdateProductClone(List<IFormFile>? imageFiles, UpdateProductModel Product, IFormFile? gameFiles)
        {
            ResponseDTO response = new();
            try
            {
                #region Kiểm tra số lượng hình ảnh
                if (imageFiles != null && (imageFiles.Count + Product.Links.Count > 7))
                {
                    response.IsSuccess = false;
                    response.Message = $"Maximum 7 image files.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra trùng lặp trong danh sách categories
                var duplicateCategories = Product.Categories
                    .GroupBy(c => c.CategoryName.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicateCategories.Any())
                {
                    response.IsSuccess = false;
                    response.Message = $"The following categories are duplicated: {string.Join(", ", duplicateCategories)}";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra xem tên product có trùng lặp trong cơ sở dữ liệu
                var existingProduct = await _db.Products
                    .AnyAsync(p => p.Name.ToLower() == Product.Name.ToLower() && p.Id != Product.Id);
                if (existingProduct)
                {
                    response.IsSuccess = false;
                    response.Message = $"The product name '{Product.Name}' is already in use.";
                    return response; // Ngưng hàm và trả về response
                }
                #endregion

                #region Kiểm tra category có tồn tại hay không
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                bool _isExist = false; // Khởi tạo biến _isExist
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessageCheckExistCategory(Product.Categories); // Gửi message

                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnCategoryResponseReceived += (response) =>
                    {
                        Console.WriteLine($"Received response: {response.CategoryName}, _IsExist: {response.IsExist}");
                        _isExist = response.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(_isExist);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist)
                    {
                        response.IsSuccess = false;
                        response.Message = "The Category is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm tra user có tồn tại hay không
                int maxRetryAttempts2 = 3; // Số lần thử lại tối đa
                int retryCount2 = 0; // Đếm số lần thử lại
                bool _isExist2 = false; // Khởi tạo biến _isExist
                bool isCompleted2 = false; // Biến đánh dấu hoàn thành

                while (retryCount2 < maxRetryAttempts2 && !isCompleted2)
                {
                    _message.SendingMessageCheckExistUserName(Product.UserName); // Gửi message

                    var tcs2 = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnUserResponseReceived += (response2) =>
                    {
                        Console.WriteLine($"_IsExist: {response2.IsExist}");
                        _isExist2 = response2.IsExist; // Gán giá trị vào biến

                        // Đánh dấu task đã hoàn thành
                        if (!tcs2.Task.IsCompleted)
                        {
                            tcs2.SetResult(_isExist2);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask2 = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask2 = await Task.WhenAny(tcs2.Task, timeoutTask2);

                    // Nếu timeout xảy ra
                    if (completedTask2 == timeoutTask2)
                    {
                        Console.WriteLine($"Attempt {retryCount2 + 1} failed due to timeout.");
                        retryCount2++; // Tăng số lần thử lại

                        if (retryCount2 >= maxRetryAttempts2)
                        {
                            response.IsSuccess = false;
                            response.Message = "The system encountered a problem while performing deletion"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Ngưng hàm và trả về response
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted2 = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (!_isExist2)
                    {
                        response.IsSuccess = false;
                        response.Message = "User is not exist"; // Trả về lỗi sau khi hết số lần thử lại
                        return response; // Ngưng hàm và trả về response
                    }
                }
                #endregion

                #region Kiểm duyệt description, name
                // Kiểm duyệt nội dung
                if (!_helper.IsContentAppropriate(Product.Description) || !_helper.IsContentAppropriate(Product.Name))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                else
                {
                    #region Kiểm duyệt hình ảnh, quét virus và update product

                    var product = await _db.Products.FindAsync(Product.Id);
                    if (product != null)
                    {

                        // Nếu không có file nào từ client, cập nhật sản phẩm không kiểm duyệt hoặc thêm link mới
                        if ((imageFiles == null || imageFiles.Count == 0) && gameFiles == null)
                        {
                            var proNoFile = await _helper.UpdateProductClone(Product);
                            response.Result = _mapper.Map<Products>(proNoFile);
                            return response;
                        }

                        // Khởi tạo ContentSafetyClient và danh sách link từ client
                        var client = new ContentSafetyClient(new Uri(initializationModel.EndpointContentSafety), new AzureKeyCredential(initializationModel.ApiKeyContentSafety));
                        var linkModel = new List<LinkModel>(Product.Links);
                        var listUnsafe = new List<bool>();

                        // Kiểm duyệt và upload mỗi hình ảnh từ imageFiles nếu có
                        if (imageFiles != null && imageFiles.Count > 0)
                        {
                            var imageTasks = imageFiles.Select(async imageFile =>
                            {
                                using var imageStream = new MemoryStream();
                                await imageFile.CopyToAsync(imageStream);
                                imageStream.Position = 0;

                                var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));
                                var request = new AnalyzeImageOptions(imageData);
                                try
                                {
                                    var responseAnalyze = await client.AnalyzeImageAsync(request);
                                    var result = responseAnalyze.Value;

                                    // Kiểm tra kết quả kiểm duyệt
                                    if (result.CategoriesAnalysis.Any(c => c.Severity > 4))
                                    {
                                        listUnsafe.Add(true);
                                        return; // Nếu hình ảnh không đạt, ngừng xử lý
                                    }

                                    // Hình ảnh đạt yêu cầu, upload lên Cloudinary và thêm link mới vào danh sách
                                    var imageLink = await _helper.UploadImageCloudinary(imageFile);
                                    linkModel.Add(new LinkModel
                                    {
                                        ProviderName = "Cloudinary",
                                        Url = imageLink,
                                        Type = LinkType.External,
                                        Status = LinkStatus.Active,
                                        Censorship = new CensorshipModel
                                        {
                                            ProviderName = "Azure Content Safety",
                                            Description = "Content Safety",
                                            Status = CensorshipStatus.Access
                                        }
                                    });
                                }
                                catch (RequestFailedException)
                                {
                                    listUnsafe.Add(true);
                                }
                            });
                            await Task.WhenAll(imageTasks);
                        }

                        // Kiểm duyệt và upload cho gameFile nếu có
                        if (gameFiles != null)
                        {
                            // Tìm link Google Drive cũ
                            var existingGoogleDriveLink = linkModel.FirstOrDefault(l => l.ProviderName == "Google Drive");

                            if (existingGoogleDriveLink != null)
                            {
                                // Xóa link cũ
                                linkModel.Remove(existingGoogleDriveLink);
                            }

                            // Check rar password của file game
                            if (!IsCorrectPassword(gameFiles, Product.WinrarPassword))
                            {
                                throw new Exception("Rar password is incorrect please enter the correct password");
                            }

                            string scan = await _helper.ScanFileForVirus(gameFiles);
                            if (scan != "OK")
                            {
                                listUnsafe.Add(true);
                            }
                            else
                            {
                                var gameLink = await _helper.UploadFileToGoogleDrive(gameFiles);
                                linkModel.Add(new LinkModel
                                {
                                    ProviderName = "Google Drive",
                                    Url = gameLink,
                                    Type = LinkType.External,
                                    Status = LinkStatus.Active,
                                    Censorship = new CensorshipModel
                                    {
                                        ProviderName = "VirusTotal",
                                        Description = "Scan Virus",
                                        Status = CensorshipStatus.Access
                                    }
                                });
                            }
                        }

                        // Kiểm tra nếu có file không hợp lệ
                        if (listUnsafe.Any())
                        {
                            response.IsSuccess = false;
                            response.Message = "Have any file not Safety";
                            return response; // Ngưng hàm và trả về response
                        }

                        // Cập nhật links của product với linkModel
                        Product.Links = linkModel;
                        var proHaveFile = await _helper.UpdateProductClone(Product);
                        response.Result = _mapper.Map<Products>(proHaveFile);// Thực hiện cập nhật sản phẩm

                        var totalRequest = await TotalRequest();
                        _message.SendingMessageStatistiscal(totalRequest.Result);
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Not found any Product";
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public bool IsCorrectPassword(IFormFile formFile, string password)
        {
            // Tạo file tạm
            string tempFilePath = Path.GetTempFileName();

            try
            {
                // Lưu file vào vị trí tạm thời
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }

                // Đường dẫn đến 7z.exe
                string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z.exe");

                // Lệnh kiểm tra file với mật khẩu
                string arguments = $"t \"{tempFilePath}\" -p\"{password}\""; // "t" để kiểm tra file, "-p" cung cấp mật khẩu

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardError.ReadToEnd(); // Đọc lỗi
                    string standardOutput = process.StandardOutput.ReadToEnd(); // Đọc output chính

                    process.WaitForExit(); // Chờ tiến trình kết thúc

                    if (process.ExitCode == 0)
                    {
                        return true; // Mật khẩu đúng (hoặc không cần mật khẩu)
                    }
                    else if (output.Contains("Wrong password"))
                    {
                        return false; // Sai mật khẩu
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Xóa file tạm
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return false;
        }

        public async Task<ResponseDTO> UpdateUserName(UpdateUserNameModel model)
        {
            ResponseDTO response = new();

            try
            {
                // Tìm tất cả sản phẩm có username trùng với oldusername
                var products = await _db.Products.Where(p => p.UserName == model.OldUserName).ToListAsync();

                if (products.Count > 0)
                {
                    // Cập nhật username của tất cả các sản phẩm tìm được
                    foreach (var product in products)
                    {
                        product.UserName = model.NewUserName;
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _db.SaveChangesAsync();

                    response.Message = "Update successfully";
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "Not found any product";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

    }
}