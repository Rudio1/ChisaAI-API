namespace ChisaApi.Application.ExpenseCategories.DataTransfers.Responses;

public sealed record ExpenseCategoryDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
