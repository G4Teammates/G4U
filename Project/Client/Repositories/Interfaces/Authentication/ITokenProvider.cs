namespace Client.Repositories.Interfaces.Authentication
{
    public interface ITokenProvider
    {
        void SetToken(string name, string token, int expiredDay, bool? isCookie = true);
        string? GetToken(string name, bool? isCookie = true);
        void ClearToken(string name);
        //string GetIdentityToken();
    }
}
