namespace ChisaApi.Application.Expenses.DataTransfer.Requests;

public sealed record UpdateExpenseDto(decimal Amount, string Category, string? Note, DateTimeOffset SpentAt);
