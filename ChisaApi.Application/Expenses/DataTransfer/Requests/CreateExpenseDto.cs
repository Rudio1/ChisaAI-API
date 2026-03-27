namespace ChisaApi.Application.Expenses.DataTransfer.Requests;

public sealed record CreateExpenseDto(decimal Amount, string Category, string? Note, DateTimeOffset SpentAt);
