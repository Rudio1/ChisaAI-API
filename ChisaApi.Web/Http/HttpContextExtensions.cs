using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChisaApi.Web.Http;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        string? sub = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return sub is not null && Guid.TryParse(sub, out Guid id) ? id : null;
    }
}
