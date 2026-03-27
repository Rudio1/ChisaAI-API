using ChisaApi.Domain.Expenses.Entities;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChisaApi.Infrastructure.Expenses.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        await _context.Expenses.AddAsync(expense, cancellationToken).ConfigureAwait(false);
    }

    public Task<Expense?> GetByIdForUserAsync(Guid expenseId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.Expenses.FirstOrDefaultAsync(
            x => x.Id == expenseId && x.UserId == userId && x.DeletedAt == null,
            cancellationToken);

    public async Task<IReadOnlyList<Expense>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<Expense> list = await _context.Expenses.AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderByDescending(x => x.SpentAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return list;
    }
}
