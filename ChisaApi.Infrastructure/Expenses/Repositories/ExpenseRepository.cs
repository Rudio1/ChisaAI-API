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
        _context.Expenses
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x => x.Id == expenseId && x.UserId == userId && x.DeletedAt == null,
                cancellationToken);

    public async Task<IReadOnlyList<Expense>> ListByUserAsync(
        Guid userId,
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Expense> query = _context.Expenses.AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId && x.DeletedAt == null);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (startDate.HasValue)
        {
            DateTimeOffset start = new(
                startDate.Value.Year,
                startDate.Value.Month,
                startDate.Value.Day,
                0,
                0,
                0,
                TimeSpan.Zero);
            query = query.Where(x => x.SpentAt >= start);
        }

        if (endDate.HasValue)
        {
            DateTimeOffset endExclusive = new DateTimeOffset(
                endDate.Value.Year,
                endDate.Value.Month,
                endDate.Value.Day,
                0,
                0,
                0,
                TimeSpan.Zero).AddDays(1);
            query = query.Where(x => x.SpentAt < endExclusive);
        }

        List<Expense> list = await query
            .OrderByDescending(x => x.SpentAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return list;
    }
}
