using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Client.Models;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Newtonsoft.Json;
using static Client.Utility.StaticTypeApi;

namespace Client.Repositories.Services
{
    public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ITokenProvider _tokenProvider = tokenProvider;


        public async Task<ResponseModel> SendAsync(RequestModel requestDTO, bool withBearer = true)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("G4TAPI");
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                //Token
                if (withBearer)
                {

                    var token = _tokenProvider.GetToken();
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDTO.Url!);
                if (requestDTO.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(requestDTO.Data), Encoding.UTF8, "application/json");
                }

                HttpResponseMessage? apiResponse = null;

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

                apiResponse = await client.SendAsync(message);

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Sever Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseModel = JsonConvert.DeserializeObject<ResponseModel>(apiContent);
                        return apiResponseModel;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseModel()
                {
                    Message = ex.Message,
                    IsSuccess = false,
                };
                return dto;
            }
        }

        //public Task<ResponseModel> SendAsync(RequestModel requestDTO, bool withBearer = true)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
