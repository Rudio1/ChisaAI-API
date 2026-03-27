namespace ChisaApi.Application.Auth;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(Guid userId, string phoneNumberE164);
    TimeSpan AccessTokenLifetime { get; }
}
