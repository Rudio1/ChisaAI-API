using ChisaApi.Domain.Expenses.Entities;

namespace ChisaApi.Domain.Expenses.Interfaces;

public interface IExpenseCategoryRepository
{
    Task AddAsync(ExpenseCategory category, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsNameForUserAsync(Guid userId, string name, Guid? excludeCategoryId, CancellationToken cancellationToken = default);
    Task<int> CountActiveExpensesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<ExpenseCategory?> GetByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
    Task<ExpenseCategory?> GetTrackedByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategory>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ExpenseCategory?> FindActiveByNameForUserAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default);
}
