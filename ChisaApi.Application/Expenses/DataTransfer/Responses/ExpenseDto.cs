namespace ChisaApi.Application.Expenses.DataTransfer.Responses;

public sealed record ExpenseDto(
    Guid Id,
    decimal Amount,
    string Category,
    string? Note,
    DateTimeOffset SpentAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);
