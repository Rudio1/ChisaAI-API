namespace ChisaApi.Application.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ChisaApi";
    public string Audience { get; set; } = "ChisaApi.Clients";
    public string SigningKey { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 60;
}
