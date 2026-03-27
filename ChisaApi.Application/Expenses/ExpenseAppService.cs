using AutoMapper;
using ChisaApi.Application.Abstractions;
using ChisaApi.Application.Expenses.DataTransfer.Requests;
using ChisaApi.Application.Expenses.DataTransfer.Responses;
using ChisaApi.Domain.Expenses.Entities;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Domain.Expenses.ServiceDomain;

namespace ChisaApi.Application.Expenses;

public sealed class ExpenseAppService
{
    private readonly IExpenseRepository _expenses;
    private readonly IExpenseCategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ExpenseDomainService _expenseDomain;

    public ExpenseAppService(
        IExpenseRepository expenses,
        IExpenseCategoryRepository categories,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ExpenseDomainService expenseDomain)
    {
        _expenses = expenses;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _expenseDomain = expenseDomain;
    }

    public async Task<ExpenseDto> CreateAsync(Guid userId, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        _expenseDomain.ValidateExpenseInput(dto.Amount, dto.CategoryId, dto.Note);

        if (!await _categories.ExistsActiveForUserAsync(dto.CategoryId, userId, cancellationToken).ConfigureAwait(false))
            throw new ArgumentException("Categoria inválida ou não pertence ao utilizador.");

        Expense expense = _mapper.Map<Expense>(dto);
        expense.Id = Guid.NewGuid();
        expense.UserId = userId;

        await _expenses.AddAsync(expense, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<IReadOnlyList<ExpenseDto>> ListAsync(
        Guid userId,
        ListExpensesQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.StartDate.Value > query.EndDate.Value)
            throw new ArgumentException("startDate não pode ser posterior a endDate.");

        if (query.CategoryId.HasValue
            && !await _categories.ExistsActiveForUserAsync(query.CategoryId.Value, userId, cancellationToken).ConfigureAwait(false))
            throw new ArgumentException("Categoria inválida ou não pertence ao utilizador.");

        IReadOnlyList<Expense> list = await _expenses.ListByUserAsync(
                userId,
                query.StartDate,
                query.EndDate,
                query.CategoryId,
                cancellationToken)
            .ConfigureAwait(false);
        return _mapper.Map<List<ExpenseDto>>(list);
    }

    public async Task<ExpenseDto?> GetAsync(Guid userId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        Expense? expense = await _expenses.GetByIdForUserAsync(expenseId, userId, cancellationToken).ConfigureAwait(false);
        return expense is null ? null : _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto?> UpdateAsync(Guid userId, Guid expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        _expenseDomain.ValidateExpenseInput(dto.Amount, dto.CategoryId, dto.Note);

        if (!await _categories.ExistsActiveForUserAsync(dto.CategoryId, userId, cancellationToken).ConfigureAwait(false))
            throw new ArgumentException("Categoria inválida ou não pertence ao utilizador.");

        Expense? expense = await _expenses.GetByIdForUserAsync(expenseId, userId, cancellationToken).ConfigureAwait(false);
        if (expense is null)
            return null;

        _mapper.Map(dto, expense);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<bool> SoftDeleteAsync(Guid userId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        Expense? expense = await _expenses.GetByIdForUserAsync(expenseId, userId, cancellationToken).ConfigureAwait(false);
        if (expense is null)
            return false;

        expense.DeletedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
