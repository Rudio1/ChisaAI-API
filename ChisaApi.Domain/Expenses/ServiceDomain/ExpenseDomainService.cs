namespace ChisaApi.Domain.Expenses.ServiceDomain;

public sealed class ExpenseDomainService
{
    public const int MaxCategoryLength = 200;
    public const int MaxNoteLength = 2000;

    public void ValidateExpenseInput(decimal amount, string category, string? note)
    {
        if (amount <= 0)
            throw new ArgumentException("O valor do gasto tem de ser maior que zero.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("A categoria é obrigatória.");
        if (category.Length > MaxCategoryLength)
            throw new ArgumentException($"Categoria demasiado longa (máx. {MaxCategoryLength}).");
        if (note is { Length: > MaxNoteLength })
            throw new ArgumentException($"Nota demasiado longa (máx. {MaxNoteLength}).");
    }
}
