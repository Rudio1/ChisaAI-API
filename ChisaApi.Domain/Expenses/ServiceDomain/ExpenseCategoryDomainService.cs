namespace ChisaApi.Domain.Expenses.ServiceDomain;

public sealed class ExpenseCategoryDomainService
{
    public const int MaxNameLength = 200;

    public string NormalizeName(string name)
    {
        return name.Trim();
    }

    public void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria é obrigatório.");
        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Nome da categoria demasiado longo (máx. {MaxNameLength}).");
    }
}
