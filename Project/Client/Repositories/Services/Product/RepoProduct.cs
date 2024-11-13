using Client.Models;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces.Product; // Đảm bảo không gian tên này được sử dụng
using Client.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Repositories.Interfaces;
using IRepoProduct = Client.Repositories.Interfaces.Product.IRepoProduct;

using Microsoft.SqlServer.Server;
using Microsoft.CodeAnalysis;

using LinkModel = Client.Models.ProductDTO.LinkModel;
using CategoryModel = Client.Models.ProductDTO.CategoryModel;

using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

using System.Net.NetworkInformation;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

using System.Drawing.Printing;
using Client.Models.CategorisDTO;



namespace Client.Repositories.Services.Product
{
    public class RepoProduct : IRepoProduct // Chỉ định không gian tên đầy đủ
    {
        public readonly IBaseService _baseService;
        private readonly HttpClient _httpClient;

        public RepoProduct(IBaseService baseService, HttpClient httpClient)
        {
            _baseService = baseService;
            _httpClient = httpClient;
        }

        public async Task<ResponseModel> CreateProductAsync(
            string name,
            string description,
            decimal price,
            float discount,
            List<string> categories,
            int platform,
            int status,
            List<IFormFile> imageFiles,
            ScanFileRequest request,
            string username)
        {
            // Tạo MultipartFormDataContent để gửi dữ liệu
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(name), "name");
            formData.Add(new StringContent(description ?? string.Empty), "description");
            formData.Add(new StringContent(price.ToString()), "price");
            formData.Add(new StringContent(discount.ToString()), "discount");
            formData.Add(new StringContent(platform.ToString()), "platform");
            formData.Add(new StringContent(status.ToString()), "status");
            formData.Add(new StringContent(username), "username");

            // Thêm từng danh mục vào formData như các phần tử riêng lẻ
            if (categories != null && categories.Count > 0)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    formData.Add(new StringContent(categories[i]), $"categories[{i}]");
                }
            }

            // Thêm các tệp hình ảnh vào formData
            foreach (var file in imageFiles)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "imageFiles",
                    FileName = file.FileName
                };
                formData.Add(fileContent);
            }

            // Thêm tệp game vào formData
            if (request?.gameFile != null)
            {
                var gameFileContent = new StreamContent(request.gameFile.OpenReadStream());
                gameFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "gameFile",
                    FileName = request.gameFile.FileName
                };
                formData.Add(gameFileContent);
            }

            // Gửi yêu cầu POST thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Product"
            });

            return response;
        }

        public async Task<ResponseModel?> GetAllProductAsync(int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product?page=" + pageNumber.ToString() +"&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel?> GetProductByIdAsync(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product/" + Id
            });
        }

        public async Task<ResponseModel> UpdateProductAsync(string id,
                                                    string name,
                                                    string description,
                                                    decimal price,
                                                    int sold,
                                                    int numOfView,
                                                    int numOfLike,
                                                    int numOfDisLike,
                                                    float discount,
                                                    List<LinkModel> links,
                                                    List<string> categories,
                                                    int platform,
                                                    int status,
                                                    DateTime createdAt,
                                                    List<IFormFile>? imageFiles,
                                                    ScanFileRequest? request,
                                                    string username,
                                                    List<string> userLikes,
                                                    List<string> userDisLike)
        {
            var formData = new MultipartFormDataContent();

            // Thêm các tham số vào formData
            formData.Add(new StringContent(id), "id");
            formData.Add(new StringContent(name), "name");
            formData.Add(new StringContent(description), "description");
            formData.Add(new StringContent(price.ToString()), "price");
            formData.Add(new StringContent(sold.ToString()), "sold");
            formData.Add(new StringContent(numOfView.ToString()), "numOfView");
            formData.Add(new StringContent(numOfLike.ToString()), "numOfLike");
            formData.Add(new StringContent(numOfDisLike.ToString()), "numOfDisLike");
            formData.Add(new StringContent(discount.ToString()), "discount");
            formData.Add(new StringContent(platform.ToString()), "platform");
            formData.Add(new StringContent(status.ToString()), "status");
            formData.Add(new StringContent(createdAt.ToString("o")), "createdAt");
            formData.Add(new StringContent(username), "username");

            // Thêm từng danh mục vào formData như các phần tử riêng lẻ
            if (categories != null && categories.Count > 0)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    formData.Add(new StringContent(categories[i]), $"categories[{i}]");
                }
            }

            if (userLikes != null && userLikes.Count > 0)
            {
                for (int i = 0; i < userLikes.Count; i++)
                {
                    formData.Add(new StringContent(userLikes[i]), $"userLikes[{i}]");
                }
            }
            if (userDisLike != null && userDisLike.Count > 0)
            {
                for (int i = 0; i < userDisLike.Count; i++)
                {
                    formData.Add(new StringContent(userDisLike[i]), $"userDisLike[{i}]");
                }
            }

            // Thêm các liên kết vào formData
            if (links != null && links.Count > 0)
            {
                for (int i = 0; i < links.Count; i++)
                {
                    var linkJson = JsonConvert.SerializeObject(links[i]);
                    formData.Add(new StringContent(linkJson), $"links[{i}]"); // Gửi từng đối tượng LinkModel dưới dạng JSON
                }
            }

            // Thêm tệp hình ảnh vào formData
            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    formData.Add(fileContent, "imageFiles", file.FileName);
                }
            }

            // Thêm tệp game vào formData
            if (request?.gameFile != null)
            {
                var gameFileContent = new StreamContent(request.gameFile.OpenReadStream());
                gameFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "gameFile",
                    FileName = request.gameFile.FileName
                };
                formData.Add(gameFileContent);
            }
            // Gửi yêu cầu PUT thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Url = StaticTypeApi.APIGateWay + "/Product",
                Data = formData
            });

            return response;
        }

        public async Task<ResponseModel?> SearchProductAsync(string searchString, int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Product/search={searchString}?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }


        public async Task<ResponseModel> SortProductAsync(string sort, int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Product/sort={sort}?page=" + pageNumber.ToString() +"&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> FilterProductAsync(decimal? minrange, decimal? maxrange, int? sold, bool? Discount, int? Platform, string Category, int? pageNumber, int pageSize)
        {
            // Tạo URL cho API với các tham số truy vấn
            var url = $"{StaticTypeApi.APIGateWay}/Product/filter?" +
                      $"minRange={minrange}&" +
                      $"maxRange={maxrange}&" +
                      $"sold={sold}&" +
                      $"discount={Discount}&" +
                      $"platform={Platform}&" +
                      $"category={Category}&" +
                      $"page={pageNumber}&" +
                      $"&pageSize={pageSize}";


            // Gửi yêu cầu GET thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET, // Sử dụng GET cho yêu cầu lọc
                Url = url
            });

            return response;
        }

        public async Task<ResponseModel> DeleteProductAsync(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Product/" + Id,
            });
        }

		public string GenerateQRCode(string qrCodeUrl)
		{
			if (!string.IsNullOrEmpty(qrCodeUrl))
			{
				QRCodeGenerator qrGenerator = new QRCodeGenerator();
				QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
				PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

				byte[] qrCodeImage = qrCode.GetGraphic(20);
				return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
			}
			return string.Empty;
		}

        public async Task<ResponseModel?> GetDetailByIdAsync(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Product/detail={Id}"
            });
        }

        public async Task<ResponseModel> GetAllProductsByUserName(string userName)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Product/getAllProductByUserName/{userName}"
            });
        }

        public async Task<ResponseModel> IncreaseLike(string productId, UserLikesModel userLikes)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = userLikes,
                Url = $"{StaticTypeApi.APIGateWay}/Product/IncreaseLike/{productId}"
            });
        }

        public async Task<ResponseModel> DecreaseLike(string productId, UserDisLikesModel userDisLikes)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = userDisLikes,
                Url = $"{StaticTypeApi.APIGateWay}/Product/DecreaseLike/{productId}"
            });
        }

        public async Task<ResponseModel> ViewMore(string viewString)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Product/ViewMore/{viewString}"
            });
        }

        /*public string GenerateBarCode(long barCodeUrl)
        {
            if (barCodeUrl != null)
            {
                var barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.UPC_A;
                barcodeWriter.Options.Width = 250;
                barcodeWriter.Options.Height = 50;

                // Tạo Bitmap từ mã vạch
                var barcodeBitmap = barcodeWriter.Write(barCodeUrl.ToString());

                // Chuyển đổi Bitmap thành mảng byte[]
                using var memoryStream = new MemoryStream();
                barcodeBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                byte[] barCodeImage = memoryStream.ToArray();

                // Trả về mã vạch dưới dạng chuỗi Base64
                return $"data:image/png;base64,{Convert.ToBase64String(barCodeImage)}";
            }
            return string.Empty;
        }*/
    }
}







