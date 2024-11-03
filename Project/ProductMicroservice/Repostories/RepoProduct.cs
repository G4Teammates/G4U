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
    }
}
