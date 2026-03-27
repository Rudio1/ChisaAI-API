namespace ChisaApi.Application.Auth.DataTransfer.Requests;

public sealed record LoginDto(string PhoneNumber, string Password);
