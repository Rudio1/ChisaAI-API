using ChisaApi.Domain.Expenses.Entities;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChisaApi.Infrastructure.Expenses.Repositories;

public sealed class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _context;

    public ExpenseCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExpenseCategory category, CancellationToken cancellationToken = default)
    {
        await _context.ExpenseCategories.AddAsync(category, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsActiveForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.ExpenseCategories.AnyAsync(
            x => x.Id == categoryId && x.UserId == userId && x.DeletedAt == null,
            cancellationToken);

    public Task<bool> ExistsNameForUserAsync(
        Guid userId,
        string name,
        Guid? excludeCategoryId,
        CancellationToken cancellationToken = default) =>
        _context.ExpenseCategories.AnyAsync(
            x => x.UserId == userId
                && x.Name == name
                && x.DeletedAt == null
                && (excludeCategoryId == null || x.Id != excludeCategoryId),
            cancellationToken);

    public Task<int> CountActiveExpensesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        _context.Expenses.CountAsync(
            x => x.CategoryId == categoryId && x.DeletedAt == null,
            cancellationToken);

    public Task<ExpenseCategory?> GetByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.ExpenseCategories.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == categoryId && x.UserId == userId && x.DeletedAt == null,
            cancellationToken);

    public Task<ExpenseCategory?> GetTrackedByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.ExpenseCategories.FirstOrDefaultAsync(
            x => x.Id == categoryId && x.UserId == userId && x.DeletedAt == null,
            cancellationToken);

    public async Task<IReadOnlyList<ExpenseCategory>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<ExpenseCategory> list = await _context.ExpenseCategories.AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return list;
    }

    public Task<ExpenseCategory?> FindActiveByNameForUserAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default) =>
        _context.ExpenseCategories.AsNoTracking().FirstOrDefaultAsync(
            x => x.UserId == userId
                && x.DeletedAt == null
                && x.Name.ToLower() == name.ToLower(),
            cancellationToken);
}
