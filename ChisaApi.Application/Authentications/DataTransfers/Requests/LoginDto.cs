namespace ChisaApi.Application.Authentications.DataTransfers.Requests;

public sealed record LoginDto(string PhoneNumber, string Password);
