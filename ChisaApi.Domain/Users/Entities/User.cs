using ChisaApi.Domain.Common;

namespace ChisaApi.Domain.Users.Entities;

public sealed class User : IAuditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string PhoneNumberE164 { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
