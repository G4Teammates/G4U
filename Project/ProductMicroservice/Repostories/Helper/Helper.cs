using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.Drive;
using ProductMicroservice.Models.DTO;
using ProductMicroservice.Models.Initialization;
using ProductMicroservice.Models;
using RestSharp;
using System.Text;
using Newtonsoft.Json;
using AutoMapper;
using ProductMicroservice.DBContexts;

namespace ProductMicroservice.Repostories.Helper
{
    public class Helper : IHelper
    {
        #region declaration and initialization
        private readonly ProductDbContext _db;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;


        public Helper(ProductDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            var account = new Account(initializationModel.cloudNameCloudinary, initializationModel.apiKeyCloudinary, initializationModel.apiSecretCloudinary);
            _cloudinary = new Cloudinary(account);
        }

        #endregion
        public async Task<string> ScanFileForVirus(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "Invalid file.";
            }

            try
            {
                // Tính toán SHA-256 của file
                string fileHash;
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var hashBytes = sha256.ComputeHash(stream);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }

                // Nếu file lớn hơn 32MB, chúng ta cần sử dụng URL upload đặc biệt
                string uploadUrl = initializationModel.VirusTotalScanUrl; // mặc định là /files nếu file <= 32MB
                if (file.Length > 32 * 1024 * 1024) // Kiểm tra kích thước file (32MB)
                {
                    // Gửi GET Request để lấy URL upload đặc biệt cho file lớn
                    var getUploadUrlOptions = new RestClientOptions("https://www.virustotal.com/api/v3/files/upload_url");
                    var getUploadUrlClient = new RestClient(getUploadUrlOptions);

                    var getUploadUrlRequest = new RestRequest("", Method.Get);
                    getUploadUrlRequest.AddHeader("x-apikey", initializationModel.VirusTotalApiKey);
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
                uploadRequest.AddHeader("x-apikey", initializationModel.VirusTotalApiKey);
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
                    var reportUrl = initializationModel.VirusTotalReportUrl.Replace("{id}", fileHash); // Sử dụng SHA-256 làm id

                    var reportClientOptions = new RestClientOptions(reportUrl);
                    var reportClient = new RestClient(reportClientOptions);

                    // VirusTotal API v3 có thể cần một thời gian để xử lý quét virus, bạn có thể phải chờ vài giây trước khi lấy báo cáo
                    await Task.Delay(15000); // Chờ 15 giây để đảm bảo báo cáo sẵn sàng

                    // GET Request (Lấy báo cáo)
                    var reportRequest = new RestRequest(reportUrl, Method.Get);
                    reportRequest.AddHeader("x-apikey", initializationModel.VirusTotalApiKey);
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
                return $"Internal server error: {ex.Message}";
            }
        }
        public async Task<string> UploadImageCloudinary(IFormFile file)
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
        public async Task<string> UploadFileToGoogleDrive(IFormFile file)

        {
            // Lấy nội dung JSON của service account từ Azure Key Vault
            var serviceAccountJsonContent = initializationModel.serviceAccountJsonContent; // Sử dụng secret đã lưu trong Key Vault

            // Đường dẫn tới thư mục trên Google Drive (thay bằng ID của thư mục bạn đã tạo)
            var folderId = initializationModel.folderId; // Thay bằng ID thư mục của bạn

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
        public async Task<Products> CreateProduct(CreateProductModel Product, List<LinkModel> linkModel, string username)
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
        public async Task<Products> UpdateProduct(UpdateProductModel Product)
        {
            var upProduct = _db.Products.Find(Product.Id);
            if (upProduct != null)
            {
                upProduct.Name = Product.Name;
                upProduct.Description = Product.Description;
                upProduct.Price = Product.Price;
                upProduct.Sold = Product.Sold;
                upProduct.Interactions.NumberOfViews = Product.Interactions.NumberOfViews;
                upProduct.Interactions.NumberOfLikes = Product.Interactions.NumberOfLikes;
                upProduct.Interactions.NumberOfDisLikes = Product.Interactions.NumberOfDisLikes;
                upProduct.Interactions.UserDisLikes = Product.Interactions.UserDisLikes
                    .Select(userName=> new UserDisLikes
                    {
                        UserName = userName.UserName
                    }).ToList();
                upProduct.Interactions.UserLikes = Product.Interactions.UserLikes
                    .Select(userName => new UserLikes
                    {
                        UserName = userName.UserName
                    }).ToList();
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
    }
}
