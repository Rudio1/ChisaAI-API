namespace ChisaApi.Application.Auth.DataTransfer.Responses;

public sealed record TokenResponseDto(
    string Name,
    string AccessToken,
    int ExpiresInSeconds,
    string TokenType);
