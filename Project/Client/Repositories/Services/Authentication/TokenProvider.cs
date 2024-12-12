using Client.Repositories.Interfaces.Authentication;
using Client.Utility;

namespace Client.Repositories.Services.Authentication
{
    public class TokenProvider(IHttpContextAccessor contextAccessor) : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;


        public void ClearToken(string name)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(name);
            _contextAccessor.HttpContext?.Session.Remove(name);
        }

        public string? GetToken(string nameToken, bool? isCookie = true)
        {
            string? token = null;
            if ((bool)isCookie)
            {
                token = _contextAccessor.HttpContext?.Request.Cookies[nameToken];
            }
            else
            {
                token = _contextAccessor.HttpContext?.Session.GetString(nameToken);
            }
            return token;
        }

        public void SetToken(string name, string token, int expiredDay, bool? isCookie = true)
        {
            if ((bool)isCookie)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Bảo vệ cookie khỏi bị truy cập bởi JavaScript
                    Secure = true, // Chỉ gửi cookie qua HTTPS
                                   //SameSite = SameSiteMode.Strict, // Ngăn chặn cookie gửi từ bên thứ ba
                    Expires = DateTime.UtcNow.AddDays(expiredDay) // Đặt thời hạn hết hạn là 1 ngày
                };

                _contextAccessor.HttpContext?.Response.Cookies.Append(name, token, cookieOptions);
            }
            else
            {
                _contextAccessor.HttpContext?.Session.SetString(name, token);
            }
        }

        //public string GetIdentityToken()
        //{

        //    string? token = null;

        //    bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(".AspNetCore.Identity.Application", out token);
        //    return hasToken is true ? token : null;
        //}
    }
}
