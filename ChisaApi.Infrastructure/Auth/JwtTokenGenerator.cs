using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ChisaApi.Application.Auth;

namespace ChisaApi.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwt;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwt = jwtOptions.Value;
    }

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(_jwt.AccessTokenMinutes);

    public string CreateAccessToken(Guid userId, string phoneNumberE164)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey) || _jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey deve ter pelo menos 32 caracteres.");

        Claim[] claims =
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("phone", phoneNumberE164),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        JwtSecurityToken token = new(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
