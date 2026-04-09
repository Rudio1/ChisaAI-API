namespace ChisaApi.Application.Expenses.DataTransfers.Requests;

public sealed record UpdateExpenseDto(decimal Amount, Guid CategoryId, string? Note, DateTimeOffset SpentAt);
