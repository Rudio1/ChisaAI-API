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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ExpenseDomainService _expenseDomain;

    public ExpenseAppService(
        IExpenseRepository expenses,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ExpenseDomainService expenseDomain)
    {
        _expenses = expenses;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _expenseDomain = expenseDomain;
    }

    public async Task<ExpenseDto> CreateAsync(Guid userId, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        _expenseDomain.ValidateExpenseInput(dto.Amount, dto.Category, dto.Note);

        Expense expense = _mapper.Map<Expense>(dto);
        expense.Id = Guid.NewGuid();
        expense.UserId = userId;

        await _expenses.AddAsync(expense, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<IReadOnlyList<ExpenseDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Expense> list = await _expenses.ListByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        List<ExpenseDto> dtos = _mapper.Map<List<ExpenseDto>>(list);
        return dtos;
    }

    public async Task<ExpenseDto?> GetAsync(Guid userId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        Expense? expense = await _expenses.GetByIdForUserAsync(expenseId, userId, cancellationToken).ConfigureAwait(false);
        return expense is null ? null : _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto?> UpdateAsync(Guid userId, Guid expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        _expenseDomain.ValidateExpenseInput(dto.Amount, dto.Category, dto.Note);

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
