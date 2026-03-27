namespace ChisaApi.Application.Expenses.DataTransfer.Requests;

public sealed record CreateExpenseDto(decimal Amount, Guid CategoryId, string? Note, DateTimeOffset SpentAt);
