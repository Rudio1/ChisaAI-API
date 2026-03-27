namespace ChisaApi.Application.ExpenseCategories.DataTransfer.Responses;

public sealed record ExpenseCategoryDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
