using ChisaApi.Application.Abstractions;
using ChisaApi.Application.Authentications.DataTransfers.Requests;
using ChisaApi.Application.Authentications.DataTransfers.Responses;
using ChisaApi.Application.Authentications.Services.Interfaces;
using ChisaApi.Application.Authentications.Settings;
using ChisaApi.Application.Utilities;
using ChisaApi.Domain.Users;
using ChisaApi.Domain.Users.Entities;
using ChisaApi.Domain.Users.ServiceDomain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChisaApi.Application.Authentications.Services;

public sealed class AuthAppService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AuthSettings _settings;
    private readonly UserRegistrationDomainService _userRegistration;

    public AuthAppService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwt,
        IPasswordHasher<User> passwordHasher,
        IOptions<AuthSettings> settings,
        UserRegistrationDomainService userRegistration)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
        _settings = settings.Value;
        _userRegistration = userRegistration;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        string name = _userRegistration.NormalizeDisplayName(dto.Name);
        _userRegistration.ValidateDisplayName(name);

        string phone = PhoneNumberNormalizer.ToBrazilE164(dto.PhoneNumber);
        _userRegistration.ValidateNewPassword(dto.Password, _settings.MinimumPasswordLength);
        _userRegistration.ValidatePasswordConfirmation(dto.Password, dto.ConfirmPassword);

        if (await _users.GetByPhoneAsync(phone, cancellationToken).ConfigureAwait(false) is not null)
            throw new InvalidOperationException("Este número já está cadastrado.");

        User user = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            PhoneNumberE164 = phone,
            PasswordHash = ""
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BuildTokenResponse(user);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        string phone = PhoneNumberNormalizer.ToBrazilE164(dto.PhoneNumber);
        User? user = await _users.GetByPhoneAsync(phone, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new InvalidOperationException("Telefone ou senha incorretos.");

        PasswordVerificationResult verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Telefone ou senha incorretos.");

        return BuildTokenResponse(user);
    }

    private TokenResponseDto BuildTokenResponse(User user)
    {
        string token = _jwt.CreateAccessToken(user.Id, user.PhoneNumberE164);
        int lifetime = (int)_jwt.AccessTokenLifetime.TotalSeconds;
        return new TokenResponseDto(user.Name, token, lifetime, "Bearer");
    }
}
