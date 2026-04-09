namespace ChisaApi.Application.Authentications.Services.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(Guid userId, string phoneNumberE164);
    TimeSpan AccessTokenLifetime { get; }
}
