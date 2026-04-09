namespace ChisaApi.Application.Authentications.DataTransfers.Responses;

public sealed record TokenResponseDto(
    string Name,
    string AccessToken,
    int ExpiresInSeconds,
    string TokenType);
