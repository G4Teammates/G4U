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

namespace ProductMicroservice.Repostories
{
    public class RepoProduct : IRepoProduct
    {
        #region declaration and initialization
        private readonly ProductDbContext _db ;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        private readonly IHelper _helper;
        

        public RepoProduct(IConfiguration configuration, ProductDbContext db, IMapper mapper, IHelper helper)
        {
            _helper = helper;
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
            var account = new Account(initializationModel.cloudNameCloudinary, initializationModel.apiKeyCloudinary, initializationModel.apiSecretCloudinary);
            _cloudinary = new Cloudinary(account);
        }
        #endregion





        public async Task<ResponseDTO> UpdateProduct(List<IFormFile>? imageFiles, UpdateProductModel Product, IFormFile? gameFiles)
        {
            ResponseDTO response = new();
            try
            {
                var product = await _db.Products.FindAsync(Product.Id);
                if (product != null)
                {

                    // Nếu không có file nào từ client, cập nhật sản phẩm không kiểm duyệt hoặc thêm link mới
                    if ((imageFiles == null || imageFiles.Count == 0) && gameFiles == null)
                    {
                        var proNoFile =  await _helper.UpdateProduct(Product);
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
                                var response = await client.AnalyzeImageAsync(request);
                                var result = response.Value;

                                // Kiểm tra kết quả kiểm duyệt
                                if (result.CategoriesAnalysis.Any(c => c.Severity > 0))
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


        public async Task<ResponseDTO> GetAll(int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Pros = await _db.Products.ToListAsync();
                if (Pros != null)
                {
                    response.Result = _mapper.Map<ICollection<Products>>(Pros).ToPagedList(page, pageSize);
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
                    product.Interactions.NumberOfViews++;
                    _db.Products.Update(product);
                    _db.SaveChanges();
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
                var Products = _db.Products.AsQueryable();
                if (Products != null)
                {
                    if (sort == "ascPrice")
                    {
                        Products = Products.OrderBy(x => x.Price);
                    }
                    else if (sort == "descPrice")
                    {
                        Products = Products.OrderByDescending(x => x.Price);
                    }
                    else if (sort == "ascView")
                    {
                        Products = Products.OrderBy(x => x.Interactions.NumberOfViews);
                    }
                    else if (sort == "descView")
                    {
                        Products = Products.OrderByDescending(x => x.Interactions.NumberOfViews);
                    }
                    else if (sort == "ascLike")
                    {
                        Products = Products.OrderBy(x => x.Interactions.NumberOfLikes);
                    }
                    else if (sort == "descLike")
                    {
                        Products = Products.OrderByDescending(x => x.Interactions.NumberOfLikes);
                    }
                    else if (sort == "ascSold")
                    {
                        Products = Products.OrderBy(x => x.Sold);
                    }
                    else if (sort == "descSold")
                    {
                        Products = Products.OrderByDescending(x => x.Sold);
                    }
                    response.Result = _mapper.Map<ICollection<Products>>(Products).ToPagedList(page,pageSize);
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

        public async Task<ResponseDTO> Search(string searchstring, int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Pros = _db.Products.AsQueryable();
                if (Pros != null || !string.IsNullOrEmpty(searchstring))
                {
                    // Tìm kiếm theo tên sản phẩm
                    var resultByName = Pros.Where(x => x.Name.Contains(searchstring));
                    if (resultByName.Any())
                    {
                        response.Result = _mapper.Map<ICollection<Products>>(resultByName).ToPagedList(page, pageSize);
                    }

                    // Tìm kiếm theo tên category
                    var resultByCateName = Pros.Where(x => x.Categories.Any(c => c.CategoryName.Contains(searchstring)));
                    if (resultByCateName.Any())
                    {
                        response.Result = _mapper.Map<ICollection<Products>>(resultByCateName).ToPagedList(page, pageSize);
                    }

                    // tim kiem theo username
                    var resultByUser = Pros.Where(x => x.UserName.Contains(searchstring));
                    if (resultByUser.Any())
                    {
                        response.Result = _mapper.Map<ICollection<Products>>(resultByUser).ToPagedList(page, pageSize);
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Product or search string is null";
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
                // Bắt đầu với tất cả các sản phẩm
                var query = _db.Products.AsQueryable();

                if (query != null)
                {
                    /*response.Result = _mapper.Map<ICollection<Products>>(Pros).ToPagedList(page, pageSize);*/
                    // Lọc theo khoảng giá
                    if (minrange.HasValue && maxrange.HasValue)
                    {
                        query = query.Where(p => p.Price >= minrange.Value && p.Price <= maxrange.Value);
                    }
                    else if (minrange.HasValue)
                    {
                        query = query.Where(p => p.Price >= minrange.Value);
                    }
                    else if (maxrange.HasValue)
                    {
                        query = query.Where(p => p.Price <= maxrange.Value);
                    }

                    // Lọc theo số lượng đã bán
                    if (sold.HasValue)
                    {
                        query = query.Where(p => p.Sold >= sold.Value);
                    }

                    // Lọc theo giảm giá
                    if (Discount.HasValue)
                    {
                        if (Discount.Value)
                        {
                            query = query.Where(p => p.Discount > 0); // Lọc các sản phẩm có giảm giá
                        }
                        else
                        {
                            query = query.Where(p => p.Discount == 0); // Lọc các sản phẩm không giảm giá
                        }
                    }

                    // Lọc theo nền tảng
                    if (Platform.HasValue)
                    {
                        query = query.Where(p => (int)p.Platform == Platform.Value); // so sánh với enum PlatformType
                    }

                    // Lọc theo category
                    if (!string.IsNullOrEmpty(Category))
                    {
                        query = query.Where(p => p.Categories.Any(c => c.CategoryName.Equals(Category, StringComparison.OrdinalIgnoreCase)));
                    }

                    response.Result = _mapper.Map<ICollection<Products>>(query.ToList()).ToPagedList(page, pageSize);
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
                            if (result.CategoriesAnalysis.Any(c => c.Severity > 0))
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
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any File to create Product";
                }
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


        #region method
        private async Task<string> ScanFileForVirus(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "Invalid file.";
            }

            try
            {
                // Tính toán SHA-256 của file
                string fileHash;
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var hashBytes = sha256.ComputeHash(stream);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }

                // Nếu file lớn hơn 32MB, chúng ta cần sử dụng URL upload đặc biệt
                string uploadUrl = VirusTotalScanUrl; // mặc định là /files nếu file <= 32MB
                if (file.Length > 32 * 1024 * 1024) // Kiểm tra kích thước file (32MB)
                {
                    // Gửi GET Request để lấy URL upload đặc biệt cho file lớn
                    var getUploadUrlOptions = new RestClientOptions("https://www.virustotal.com/api/v3/files/upload_url");
                    var getUploadUrlClient = new RestClient(getUploadUrlOptions);

                    var getUploadUrlRequest = new RestRequest("", Method.Get);
                    getUploadUrlRequest.AddHeader("x-apikey", VirusTotalApiKey);
                    getUploadUrlRequest.AddHeader("accept", "application/json");

                    var uploadUrlResponse = await getUploadUrlClient.ExecuteAsync(getUploadUrlRequest);
                    if (!uploadUrlResponse.IsSuccessful)
                    {
                        return "Error getting special upload URL.";
                    }

                    var uploadUrlResult = JsonConvert.DeserializeObject<UploadUrlResult>(uploadUrlResponse.Content);
                    if (uploadUrlResult == null || string.IsNullOrEmpty(uploadUrlResult.UploadUrl))
                    {
                        return "Error getting upload URL.";
                    }

                    // Sử dụng URL đặc biệt để upload file lớn
                    uploadUrl = uploadUrlResult.UploadUrl;
                }

                // Sử dụng RestClient để gửi file tới URL đã lấy được (hoặc mặc định)
                var uploadOptions = new RestClientOptions(uploadUrl);
                var uploadClient = new RestClient(uploadOptions);

                var uploadRequest = new RestRequest("", Method.Post);

                // Thêm API key vào Header
                uploadRequest.AddHeader("x-apikey", VirusTotalApiKey);
                uploadRequest.AddHeader("accept", "application/json");

                // Thêm file vào request dưới dạng multipart/form-data
                using (var memoryStream = new MemoryStream())
                {
                    await file.OpenReadStream().CopyToAsync(memoryStream);
                    uploadRequest.AddFile("file", memoryStream.ToArray(), file.FileName, "application/octet-stream");
                }

                // Gửi yêu cầu upload file để scan virus
                var uploadResponse = await uploadClient.ExecuteAsync(uploadRequest);
                if (!uploadResponse.IsSuccessful)
                {
                    return "Error uploading file to VirusTotal.";
                }

                // Đọc phản hồi sau khi upload
                var scanResult = JsonConvert.DeserializeObject<VirusTotalScanResult>(uploadResponse.Content);

                // Nếu nhận được scan_id, tiếp tục lấy báo cáo virus
                if (scanResult != null && !string.IsNullOrEmpty(scanResult.Data?.Id))
                {
                    var scanId = scanResult.Data.Id;

                    // Gửi yêu cầu lấy báo cáo virus bằng scan_id
                    var reportUrl = VirusTotalReportUrl.Replace("{id}", fileHash); // Sử dụng SHA-256 làm id

                    var reportClientOptions = new RestClientOptions(reportUrl);
                    var reportClient = new RestClient(reportClientOptions);

                    // VirusTotal API v3 có thể cần một thời gian để xử lý quét virus, bạn có thể phải chờ vài giây trước khi lấy báo cáo
                    await Task.Delay(15000); // Chờ 15 giây để đảm bảo báo cáo sẵn sàng

                    // GET Request (Lấy báo cáo)
                    var reportRequest = new RestRequest(reportUrl, Method.Get);
                    reportRequest.AddHeader("x-apikey", VirusTotalApiKey);
                    reportRequest.AddHeader("accept", "application/json");

                    var reportResponse = await reportClient.ExecuteAsync(reportRequest);
                    if (!reportResponse.IsSuccessful)
                    {
                        return $"Error getting report: {reportResponse.Content}";
                    }

                    var reportResult = JsonConvert.DeserializeObject<VirusTotalReportResult>(reportResponse.Content);

                    return "OK";
                }

                return "Error getting scan ID from VirusTotal.";
            }
            catch (Exception ex)
            {
                return  $"Internal server error: {ex.Message}";
            }
        }
        private async Task<string> UploadImageCloudinary(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty.");

            using (var stream = file.OpenReadStream()) // Mở stream từ IFormFile
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream), // Sử dụng file.FileName
                    PublicId = Path.GetFileNameWithoutExtension(file.FileName), // Tùy chọn, để định danh file
                    Folder = "DuAnToNghiep"
                };

                var uploadResult = _cloudinary.Upload(uploadParams);
                return uploadResult.SecureUri.ToString(); // Trả về URL an toàn
            }
        }
        private async Task<string> UploadFileToGoogleDrive(IFormFile file)

        {
            // Lấy nội dung JSON của service account từ Azure Key Vault
            var serviceAccountJsonContent = _configuration["7"]; // Sử dụng secret đã lưu trong Key Vault

            // Đường dẫn tới thư mục trên Google Drive (thay bằng ID của thư mục bạn đã tạo)
            var folderId = _configuration["8"]; // Thay bằng ID thư mục của bạn

            GoogleCredential credential;

            // Parse nội dung JSON từ secret
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serviceAccountJsonContent)))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.DriveFile);
            }

            // Tạo dịch vụ Google Drive
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DemoKiemduyet",
            });

            // Tạo đối tượng tệp để upload
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.FileName,
                Parents = new List<string> { folderId } // Chỉ định thư mục cha
            };

            using (var memoryStream = new MemoryStream())
            {
                await file.OpenReadStream().CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var request = service.Files.Create(fileMetadata, memoryStream, file.ContentType);
                request.Fields = "id, webViewLink"; // Lấy ID và liên kết xem web của tệp
                var fileResult = await request.UploadAsync();

                if (fileResult.Status != UploadStatus.Completed)
                {
                    throw new Exception("Error uploading file to Google Drive.");
                }

                var uploadedFile = request.ResponseBody;

                // Đặt quyền công khai cho tệp vừa upload
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Role = "reader", // Đặt quyền đọc
                    Type = "anyone"  // Bất kỳ ai có đường link đều có thể xem
                };

                var permissionRequest = service.Permissions.Create(permission, uploadedFile.Id);
                await permissionRequest.ExecuteAsync(); // Áp dụng quyền công khai

                return uploadedFile.WebViewLink; // Trả về liên kết xem tệp
            }
        }
        private async Task<Products> CreateProduct(CreateProductModel Product, List<LinkModel> linkModel ,string username)
        {

            var newProduct = new ProductModel()
            {
                Interactions = new InteractionModel(),
                Categories = Product.Categories.ToList(),
                Name = Product.Name,
                Description = Product.Description,
                Price = Product.Price,
                Discount = Product.Discount,
                Platform = Product.Platform,
                Status = Product.Status,
                Links = linkModel,
                UserName = username

			};
            var productEntity = _mapper.Map<Products>(newProduct);
            await _db.AddAsync(productEntity);
            await _db.SaveChangesAsync();
            // Kiểm tra xem danh sách categories có dữ liệu không
            Console.WriteLine($"Categories count in productEntity before save: {productEntity.Categories?.Count}");
            return productEntity;
        }
        private async Task<Products> UpdateProduct(UpdateProductModel Product)
        {
            var upProduct = await GetById(Product.Id);
            if (upProduct != null)
            {
                upProduct.Name = Product.Name;
                upProduct.Description = Product.Description;
                upProduct.Price = Product.Price;
                upProduct.Sold = Product.Sold;
                upProduct.Interactions.NumberOfViews = Product.Interactions.NumberOfViews;
                upProduct.Interactions.NumberOfLikes = Product.Interactions.NumberOfLikes;
                upProduct.Discount = Product.Discount;
                // Chuyển đổi danh sách categories
                upProduct.Categories = Product.Categories
                    .Select(category => new Categories // Giả sử Categories là kiểu dữ liệu mà bạn cần
                    {
                        // Ánh xạ các thuộc tính cần thiết từ category
                        CategoryName = category.CategoryName // Điều chỉnh tùy thuộc vào các thuộc tính của bạn
                    }).ToList();
                upProduct.Platform = Product.Platform;
                upProduct.Status = Product.Status;
                upProduct.CreatedAt = Product.CreatedAt;
                upProduct.UpdatedAt = DateTime.UtcNow;
                upProduct.UserName = Product.UserName;
                upProduct.Links = _mapper.Map<ICollection<Link>>(Product.Links);
            };

            var productEntity = _mapper.Map<Products>(upProduct);
             _db.Update(productEntity);
            await _db.SaveChangesAsync();
            return productEntity;
        }

        public async Task<ResponseDTO> GetAllProductsByUserName(string userName)
        {
            ResponseDTO response = new();
            try
            {
                var products = await _db.Products.Where(p => p.UserName == userName).ToListAsync();

                if (products.Count == 0)
                {
                    throw new Exception("This user doesn't have any product");
                }

                response.Message = "Get All Products Success";
                response.Result = products;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
        #endregion

    }
}
