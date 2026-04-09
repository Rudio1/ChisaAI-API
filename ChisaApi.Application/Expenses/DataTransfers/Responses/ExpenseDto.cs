namespace ChisaApi.Application.Expenses.DataTransfers.Responses;

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
