namespace DarkMessApp.Utils;

public class JwtTokenHandler
{
    private static readonly string _tokenKey = "jwt";
    public static async Task<string?> GetToken()
    {
        return await SecureStorage.Default.GetAsync(_tokenKey);
    }
    public static async Task SetToken(String token)
    {
        if (!string.IsNullOrWhiteSpace(token)) await SecureStorage.Default.SetAsync(_tokenKey, token);
    }
    public static void RemoveToken()
    {
        SecureStorage.Default.Remove(_tokenKey);
    }
}