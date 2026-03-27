using ChisaApi.Domain.Expenses.Entities;

namespace ChisaApi.Domain.Expenses.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> ListByUserAsync(
        Guid userId,
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? categoryId,
        CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdForUserAsync(Guid expenseId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
}
