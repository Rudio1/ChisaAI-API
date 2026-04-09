namespace ChisaApi.Application.Expenses.DataTransfers.Requests;

public sealed record CreateExpenseDto(decimal Amount, Guid CategoryId, string? Note, DateTimeOffset SpentAt);
