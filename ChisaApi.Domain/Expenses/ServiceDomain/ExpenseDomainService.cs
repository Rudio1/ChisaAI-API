namespace ChisaApi.Domain.Expenses.ServiceDomain;

public sealed class ExpenseDomainService
{
    public const int MaxNoteLength = 2000;

    public void ValidateExpenseInput(decimal amount, Guid categoryId, string? note)
    {
        if (amount <= 0)
            throw new ArgumentException("O valor do gasto tem de ser maior que zero.");
        if (categoryId == Guid.Empty)
            throw new ArgumentException("A categoria é obrigatória.");
        if (note is { Length: > MaxNoteLength })
            throw new ArgumentException($"Nota demasiado longa (máx. {MaxNoteLength}).");
    }
}
