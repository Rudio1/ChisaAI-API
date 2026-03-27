namespace ChisaApi.Application.Expenses.DataTransfer.Requests;

public sealed record UpdateExpenseDto(decimal Amount, Guid CategoryId, string? Note, DateTimeOffset SpentAt);
