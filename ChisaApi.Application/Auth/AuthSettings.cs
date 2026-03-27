namespace ChisaApi.Application.Auth;

public sealed class AuthSettings
{
    public const string SectionName = "Auth";

    public int MinimumPasswordLength { get; set; } = 8;
}
