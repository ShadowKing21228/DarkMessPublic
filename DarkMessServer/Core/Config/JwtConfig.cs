namespace DarkMessServer.Core.Config;

public static class JwtConfig
{
    public static bool Issuer { get; private set; } = false;
    public static bool Audience { get; private set; } = false;
    public static bool Lifetime { get; private set; } = true;
    public static string SecretKey { get; private set; } = string.Empty;
    public static int ExpirationMinutes { get; private set; }

    public static void Init(IConfiguration config)
    {
        Issuer = bool.Parse(config["Jwt:Issuer"] ?? "false");
        Audience = bool.Parse(config["Jwt:Audience"] ?? "false");
        Lifetime = bool.Parse(config["Jwt:Lifetime"] ?? "true");
        SecretKey = config["Jwt:SecretKey"] ?? "";
        ExpirationMinutes = int.Parse(config["Jwt:ExpirationMinutes"] ?? "60");
    }
}   