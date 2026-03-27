using System.Text.Json.Serialization;

namespace ChisaApi.Application.Auth.DataTransfer.Requests;

public sealed record RegisterDto(
    string Name,
    string PhoneNumber,
    string Password,
    [property: JsonPropertyName("confirm_password")] string ConfirmPassword);
