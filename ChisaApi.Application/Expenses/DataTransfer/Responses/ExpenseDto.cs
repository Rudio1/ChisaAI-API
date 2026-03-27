namespace ChisaApi.Application.Expenses.DataTransfer.Responses;

public sealed record ExpenseDto(
    Guid Id,
    decimal Amount,
    Guid CategoryId,
    string CategoryName,
    string? Note,
    DateTimeOffset SpentAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);
