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

namespace ProductMicroservice.Repostories
{
    public class RepoProduct : IRepoProduct
    {
        #region declaration and initialization
        private readonly ProductDbContext _db ;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        private string cloudNameCloudinary = "dl97ondjc";
        private string apiKeyCloudinary => _configuration["5"];
        private string apiSecretCloudinary => _configuration["6"];
        private string EndpointContentSafety => _configuration["10022002"];
        private string ApiKeyContentSafety => _configuration["19102001"];
        private string VirusTotalApiKey => _configuration["4"];
        private string VirusTotalScanUrl = "https://www.virustotal.com/api/v3/files";
        private string VirusTotalReportUrl = "https://www.virustotal.com/api/v3/files/{id}";
        

        public RepoProduct(IConfiguration configuration, ProductDbContext db, IMapper mapper)
        {
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
            var account = new Account(cloudNameCloudinary, apiKeyCloudinary, apiSecretCloudinary);
            _cloudinary = new Cloudinary(account);
        }
        #endregion





        public async Task<Products> UpdateProduct(List<IFormFile> imageFiles, UpdateProductModel Product, IFormFile gameFiles)
        {
            // Kiểm tra xem danh sách file có tồn tại và có ít nhất một file
            if (imageFiles == null || imageFiles.Count == 0)
            {
                throw new ArgumentException("No files provided.");
            }
            try
            {
                Products productEntity = null;
                // Tạo đối tượng ContentSafetyClient với thông tin cấu hình
                var client = new ContentSafetyClient(new Uri(EndpointContentSafety), new AzureKeyCredential(ApiKeyContentSafety));

                // Danh sách để chứa kết quả kiểm duyệt của từng file
                var moderationResults = new List<object>();

                // Tạo một danh sách các đối tượng Link
                var linkModel = new List<LinkModel>();

                // Biến để theo dõi nếu phát hiện có bất kỳ file nào không hợp lệ
                /*bool hasUnsafeFile = false;*/

                var ListUnsafe = new List<bool>();

                // Sử dụng xử lý song song để kiểm duyệt nhiều file cùng lúc
                var tasks = imageFiles.Select<IFormFile, Task<object>>(async imageFile =>
                {
                    /*if (hasUnsafeFile) return null;*/ // Nếu đã phát hiện file không hợp lệ, dừng toàn bộ quá trình

                    // Đọc dữ liệu hình ảnh từ file upload vào MemoryStream
                    using (var imageStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(imageStream); // Sao chép dữ liệu từ file upload vào stream
                        imageStream.Position = 0; // Đặt vị trí đọc của stream về đầu

                        // Tạo đối tượng ContentSafetyImageData từ dữ liệu hình ảnh
                        var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));

                        // Tạo đối tượng yêu cầu kiểm duyệt hình ảnh
                        var request = new AnalyzeImageOptions(imageData);

                        // Gửi yêu cầu kiểm duyệt hình ảnh lên Azure Content Safety
                        Response<AnalyzeImageResult> response;
                        try
                        {
                            response = await client.AnalyzeImageAsync(request); // Thực hiện async
                        }
                        catch (RequestFailedException ex)
                        {
                            /*hasUnsafeFile = true;*/
                            ListUnsafe.Add(true);
                            return new { Error = $"Analyze image failed. Status code: {ex.Status}, Error code: {ex.ErrorCode}, Error message: {ex.Message}" };
                        }

                        // Lấy kết quả kiểm duyệt từ dịch vụ
                        var result = response.Value;
                        var hateSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity ?? 0;
                        var selfHarmSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity ?? 0;
                        var sexualSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity ?? 0;
                        var violenceSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity ?? 0;

                        if (hateSeverity > 0 || selfHarmSeverity > 0 || sexualSeverity > 0 || violenceSeverity > 0)
                        {
                            ListUnsafe.Add(true);
                            return new { Message = "Hình ảnh không hợp lệ." };
                        }
                        else if (ListUnsafe.Count == 0)
                        {
                            // Khởi tạo biến chứa thông báo

                            string CensorProviderName = "Azure Content Safety";
                            string CensorDescription = "Content Safety";
                            int CensorStatus = 1;
                            string LinkProviderName = "Cloudinary";
                            int Linkype = 1;
                            int Linkstatus = 0;

                            // Lưu hình ảnh lên Google Drive và Cloudinary
                            var imageLink = await UploadImageCloudinary(imageFile);
                            /*var DriveLink = await UploadFileToGoogleDrive(imageFile);*/


                            var newCensor = new CensorshipModel()
                            {
                                ProviderName = CensorProviderName,
                                Description = CensorDescription,
                                Status = (CensorshipStatus)CensorStatus
                            };

                            // Giả định rằng bạn đang tạo một hoặc nhiều link từ LinkModel
                            var link = new LinkModel
                            {                               
                                ProviderName = LinkProviderName,
                                Url = imageLink,
                                Type = (LinkType)Linkype,
                                Status = (LinkStatus)Linkstatus,
                                Censorship = newCensor
                            };

                            // Thêm đối tượng Link vào danh sách
                            linkModel.Add(link);
                            return null; // Nếu hình ảnh an toàn, trả về null
                        }
                        else { return "Error"; }
                    }
                });

                string scan = await ScanFileForVirus(gameFiles);
                if (scan != "OK")
                {
                    ListUnsafe.Add(true);
                }
                else if (ListUnsafe.Count == 0)
                {
                    string CensorProviderName = "VirusTotal";
                    string CensorDescription = "Scan Virus";
                    int CensorStatus = 1;
                    string LinkProviderName = "Google Drive";
                    int Linkype = 1;
                    int Linkstatus = 0;

                    string linkgame = await UploadFileToGoogleDrive(gameFiles);

                    var newCensor = new CensorshipModel()
                    {
                        ProviderName = CensorProviderName,
                        Description = CensorDescription,
                        Status = (CensorshipStatus)CensorStatus
                    };

                    // Giả định rằng bạn đang tạo một hoặc nhiều link từ LinkModel
                    var link = new LinkModel
                    {
                        ProviderName = LinkProviderName,
                        Url = linkgame,
                        Type = (LinkType)Linkype,
                        Status = (LinkStatus)Linkstatus,
                        Censorship = newCensor
                    };

                    // Thêm đối tượng Link vào danh sách
                    linkModel.Add(link);
                }


                // Chờ tất cả các tác vụ xử lý hoàn tất
                await Task.WhenAll(tasks);

                if (ListUnsafe.Count == 0)
                {                 // Sau khi đã xử lý tất cả hình ảnh, tạo sản phẩm một lần
                    productEntity = await UpdateProduct(Product, linkModel);

                    // Trả về kết quả cho client
                    return productEntity;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Internal server error: {ex.Message}");
            }
        }

        public IEnumerable<Products> Products => _db.Products.ToList();
        public async Task<Products> GetById(string id) => _db.Products.Find(id);
        public async void DeleteProduct(string id)
        {
            var Product = await GetById(id);
            _db.Products.Remove(Product);
            _db.SaveChanges();
        }

        public IEnumerable<Products> Sort(string sort)
        {
            var Products = _db.Products.AsQueryable();

            if (sort == "ascPrice")
            {
                Products = Products.OrderBy(x => x.Price);
            }
            else if (sort == "descPrice")
            {
                Products = Products.OrderByDescending(x => x.Price);
            }
            else if(sort == "ascView")
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
            if (sort == "ascSold")
            {
                Products = Products.OrderBy(x => x.Sold);
            }
            else if (sort == "descSold")
            {
                Products = Products.OrderByDescending(x => x.Sold);
            }
            return Products;

        }

        public IEnumerable<Products> Search(string searchstring)
        {
            var Products = _db.Products.AsQueryable();


            if (!string.IsNullOrEmpty(searchstring))
            {
                // Tìm kiếm theo tên sản phẩm
                var resultByName = Products.Where(x => x.Name.Contains(searchstring));
                if (resultByName.Any())
                {
                    return resultByName;
                }

                // Tìm kiếm theo tên category
                var resultByCateName = Products.Where(x => x.Categories.Any(c => c.CategoryName.Contains(searchstring)));
                if (resultByCateName.Any())
                {
                    return resultByCateName;
                }

				// tim kiem theo username
				var resultByUser = Products.Where(x => x.UserName.Contains(searchstring));
				if (resultByCateName.Any())
				{
					return resultByCateName;
				}
			}

            // Nếu không có kết quả nào khớp với điều kiện tìm kiếm, trả về danh sách trống
            return new List<Products>();
        }
		public IEnumerable<Products> Filter(decimal? minrange, decimal? maxrange, int? sold, bool? Discount, int? Platform, string? Category)
		{
			// Bắt đầu với tất cả các sản phẩm
			var query = _db.Products.AsQueryable();

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
				query = query.Where(p => p.Categories.Any(c => c.CategoryName.Contains(Category)));
			}

			// Trả về danh sách sản phẩm sau khi áp dụng tất cả các bộ lọc
			return query.ToList();
		}

		public async Task<Products> Moderate(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username)
        {
            // Kiểm tra xem danh sách file có tồn tại và có ít nhất một file
            if (imageFiles == null || imageFiles.Count == 0)
            {
                throw new ArgumentException("No files provided.");
            }
            try
            {
                Products productEntity= null;
                // Tạo đối tượng ContentSafetyClient với thông tin cấu hình
                var client = new ContentSafetyClient(new Uri(EndpointContentSafety), new AzureKeyCredential(ApiKeyContentSafety));

                // Danh sách để chứa kết quả kiểm duyệt của từng file
                var moderationResults = new List<object>();

                // Tạo một danh sách các đối tượng Link
                var linkModel = new List<LinkModel>();

                // Biến để theo dõi nếu phát hiện có bất kỳ file nào không hợp lệ
                /*bool hasUnsafeFile = false;*/

                var ListUnsafe = new List<bool>();

                // Sử dụng xử lý song song để kiểm duyệt nhiều file cùng lúc
                var tasks = imageFiles.Select<IFormFile, Task<object>>(async imageFile => 
                {
                    /*if (hasUnsafeFile) return null;*/ // Nếu đã phát hiện file không hợp lệ, dừng toàn bộ quá trình

                    // Đọc dữ liệu hình ảnh từ file upload vào MemoryStream
                    using (var imageStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(imageStream); // Sao chép dữ liệu từ file upload vào stream
                        imageStream.Position = 0; // Đặt vị trí đọc của stream về đầu

                        // Tạo đối tượng ContentSafetyImageData từ dữ liệu hình ảnh
                        var imageData = new ContentSafetyImageData(BinaryData.FromBytes(imageStream.ToArray()));

                        // Tạo đối tượng yêu cầu kiểm duyệt hình ảnh
                        var request = new AnalyzeImageOptions(imageData);

                        // Gửi yêu cầu kiểm duyệt hình ảnh lên Azure Content Safety
                        Response<AnalyzeImageResult> response;
                        try
                        {
                            response = await client.AnalyzeImageAsync(request); // Thực hiện async
                        }
                        catch (RequestFailedException ex)
                        {
                            /*hasUnsafeFile = true;*/
                            ListUnsafe.Add(true);
                            return new { Error = $"Analyze image failed. Status code: {ex.Status}, Error code: {ex.ErrorCode}, Error message: {ex.Message}" };
                        }

                        // Lấy kết quả kiểm duyệt từ dịch vụ
                        var result = response.Value;
                        var hateSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity ?? 0;
                        var selfHarmSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity ?? 0;
                        var sexualSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity ?? 0;
                        var violenceSeverity = result.CategoriesAnalysis.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity ?? 0;

                        if (hateSeverity > 0 || selfHarmSeverity > 0 || sexualSeverity > 0 || violenceSeverity > 0)
                        {
                            ListUnsafe.Add(true);
                            return new { Message = "Hình ảnh không hợp lệ." };
                        }
                        else if (ListUnsafe.Count == 0) 
                        {
                            // Khởi tạo biến chứa thông báo

                            string CensorProviderName = "Azure Content Safety";
                            string CensorDescription = "Content Safety";
                            int CensorStatus = 1;
                            string LinkProviderName = "Cloudinary";
                            int Linkype = 1;
                            int Linkstatus = 0;

                            // Lưu hình ảnh lên Google Drive và Cloudinary
                            var imageLink = await UploadImageCloudinary(imageFile);
                            /*var DriveLink = await UploadFileToGoogleDrive(imageFile);*/


                            var newCensor = new CensorshipModel()
                            {
                                ProviderName = CensorProviderName,
                                Description = CensorDescription,
                                Status = (CensorshipStatus)CensorStatus
                            };

                            // Giả định rằng bạn đang tạo một hoặc nhiều link từ LinkModel
                            var link = new LinkModel
                            {
                                ProviderName = LinkProviderName,
                                Url = imageLink,
                                Type = (LinkType)Linkype,
                                Status = (LinkStatus)Linkstatus,
                                Censorship = newCensor
                            };

                            // Thêm đối tượng Link vào danh sách
                            linkModel.Add(link);
                            return null; // Nếu hình ảnh an toàn, trả về null
                        }   
                        else { return "Error"; }
                    }
                });

                string scan = await ScanFileForVirus(gameFiles);
                if (scan != "OK")
                {
                    ListUnsafe.Add(true);
                }
                else if (ListUnsafe.Count == 0)
                {
                    string CensorProviderName = "VirusTotal";
                    string CensorDescription = "Scan Virus";
                    int CensorStatus = 1;
                    string LinkProviderName = "Google Drive";
                    int Linkype = 1;
                    int Linkstatus = 0;

                    string linkgame = await UploadFileToGoogleDrive(gameFiles);

                    var newCensor = new CensorshipModel()
                    {
                        ProviderName = CensorProviderName,
                        Description = CensorDescription,
                        Status = (CensorshipStatus)CensorStatus
                    };

                    // Giả định rằng bạn đang tạo một hoặc nhiều link từ LinkModel
                    var link = new LinkModel
                    {
                        ProviderName = LinkProviderName,
                        Url = linkgame,
                        Type = (LinkType)Linkype,
                        Status = (LinkStatus)Linkstatus,
                        Censorship = newCensor
                    };

                    // Thêm đối tượng Link vào danh sách
                    linkModel.Add(link);
                }


                // Chờ tất cả các tác vụ xử lý hoàn tất
                await Task.WhenAll(tasks);
                
                if (ListUnsafe.Count == 0)
                {                 // Sau khi đã xử lý tất cả hình ảnh, tạo sản phẩm một lần
                    productEntity = await CreateProduct(Product, linkModel, username);

                    // Trả về kết quả cho client
                    return productEntity;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Internal server error: {ex.Message}");
            }
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
        private async Task<Products> UpdateProduct(UpdateProductModel Product, List<LinkModel> linkModel)
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
                upProduct.Categories = _mapper.Map<ICollection<Categories>>(Product.Categories);
                upProduct.Platform = Product.Platform;
                upProduct.Status = Product.Status;
                upProduct.CreatedAt = Product.CreatedAt;
                upProduct.UpdatedAt = DateTime.UtcNow;
                upProduct.UserName = Product.UserName;
                upProduct.Links = _mapper.Map<ICollection<Link>>(linkModel);

            };

            var productEntity = _mapper.Map<Products>(upProduct);
             _db.Update(productEntity);
            await _db.SaveChangesAsync();
            return productEntity;
        }
        #endregion

    }
}
