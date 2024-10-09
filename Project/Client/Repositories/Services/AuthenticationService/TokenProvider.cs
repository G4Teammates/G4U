using Client.Repositories.Interfaces.Authentication;
using Client.Utility;

namespace Client.Repositories.Services.AuthenticationService
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
            _contextAccessor.HttpContext?.Response.Cookies.Append(StaticTypeApi.TokenCookie, token);
        }
        //public string GetIdentityToken()
        //{

        //    string? token = null;

        //    bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(".AspNetCore.Identity.Application", out token);
        //    return hasToken is true ? token : null;
        //}
    }
}
