namespace Client.Repositories.Interfaces.Authentication
{
    public interface ITokenProvider
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
        //string GetIdentityToken();
    }
}
