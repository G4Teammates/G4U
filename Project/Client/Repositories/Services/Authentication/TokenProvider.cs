using Client.Repositories.Interfaces.Authentication;
using Client.Utility;

namespace Client.Repositories.Services.Authentication
{
    public class TokenProvider(IHttpContextAccessor contextAccessor) : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;


        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(StaticTypeApi.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;

            bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticTypeApi.TokenCookie, out token);

            return hasToken is true ? token : null;
        }

        public void SetToken(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Bảo vệ cookie khỏi bị truy cập bởi JavaScript
                Secure = true, // Chỉ gửi cookie qua HTTPS
                //SameSite = SameSiteMode.Strict, // Ngăn chặn cookie gửi từ bên thứ ba
                Expires = DateTime.UtcNow.AddDays(1) // Đặt thời hạn hết hạn là 1 ngày
            };

            _contextAccessor.HttpContext?.Response.Cookies.Append(StaticTypeApi.TokenCookie, token, cookieOptions);
        }

        //public string GetIdentityToken()
        //{

        //    string? token = null;

        //    bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(".AspNetCore.Identity.Application", out token);
        //    return hasToken is true ? token : null;
        //}
    }
}
