﻿using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Client.Models;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Newtonsoft.Json;
using static Client.Utility.StaticTypeApi;

namespace Client.Repositories.Services
{
	public class BaseService : IBaseService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ITokenProvider _tokenProvider;

		public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
		{
			_httpClientFactory = httpClientFactory;
			_tokenProvider = tokenProvider;
		}

		public async Task<ResponseModel> SendAsync(RequestModel requestDTO, bool withBearer = true)
		{
			try
			{
				// Kiểm tra URL
				if (string.IsNullOrEmpty(requestDTO.Url))
				{
					return new ResponseModel { IsSuccess = false, Message = "URL không hợp lệ." };
				}
				// Ghi log thông tin yêu cầu
				Console.WriteLine($"Gửi yêu cầu đến: {requestDTO.Url}");
				Console.WriteLine($"Phương thức: {requestDTO.ApiType}");
				Console.WriteLine($"Dữ liệu: {JsonConvert.SerializeObject(requestDTO.Data)}");

				HttpClient client = _httpClientFactory.CreateClient("G4TAPI");
				HttpRequestMessage message = new();
				message.Headers.Add("Accept", "application/json");

				// Thêm Token nếu cần
				if (withBearer)
				{
                    bool rememberMe = Convert.ToBoolean(_tokenProvider.GetToken("RememberMe"));
                    string token;

                    if (rememberMe)
                    {
                        token = _tokenProvider.GetToken("JWT");
                    }
                    else
                    {
                        token = _tokenProvider.GetToken("JWT", false);
                    }
                    if (!string.IsNullOrEmpty(token))
					{
						message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					}
				}

				message.RequestUri = new Uri(requestDTO.Url);

				// Kiểm tra xem dữ liệu có phải là MultipartFormDataContent
				if (requestDTO.Data is MultipartFormDataContent formData)
				{
					message.Content = formData; // Sử dụng MultipartFormDataContent trực tiếp
				}
				else if (requestDTO.Data != null)
				{
					// Nếu không phải là multipart, có thể log hoặc xử lý một cách phù hợp
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDTO.Data), Encoding.UTF8, "application/json");
				}

				// Xác định phương thức API
				switch (requestDTO.ApiType)
				{
					case ApiType.POST:
						message.Method = HttpMethod.Post;
						break;
					case ApiType.DELETE:
						message.Method = HttpMethod.Delete;
						break;
					case ApiType.PUT:
						message.Method = HttpMethod.Put;
						break;
					default:
						message.Method = HttpMethod.Get;
						break;
				}


				// Gửi yêu cầu đến API
				HttpResponseMessage apiResponse = await client.SendAsync(message);


				// Xử lý phản hồi từ API
				switch (apiResponse.StatusCode)
				{
					case HttpStatusCode.NotFound:
						return new ResponseModel { IsSuccess = false, Message = "Không tìm thấy" };
					case HttpStatusCode.Forbidden:
						return new ResponseModel { IsSuccess = false, Message = "Truy cập bị từ chối" };
					case HttpStatusCode.Unauthorized:
						return new ResponseModel { IsSuccess = false, Message = "Không có quyền truy cập" };
					case HttpStatusCode.InternalServerError:
						return new ResponseModel { IsSuccess = false, Message = "Lỗi máy chủ nội bộ" };
					default:
						var apiContent = await apiResponse.Content.ReadAsStringAsync();
						var apiResponseModel = JsonConvert.DeserializeObject<ResponseModel>(apiContent);

						// Kiểm tra xem phản hồi có hợp lệ không
						
						if (apiResponseModel == null)
						{
							return new ResponseModel { IsSuccess = false, Message = "Phản hồi từ API không hợp lệ." };
						}

						return apiResponseModel;
				}
			}
			catch (Exception ex)
			{
				// Ghi lại lỗi chi tiết nếu cần thiết
				return new ResponseModel
				{
					Message = ex.Message,
					IsSuccess = false,
				};
			}
		}
	}
}
