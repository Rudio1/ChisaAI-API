using AutoMapper;
using ChisaApi.Application.Abstractions;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Requests;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Responses;
using ChisaApi.Domain.Expenses.Entities;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Domain.Expenses.ServiceDomain;

namespace ChisaApi.Application.ExpenseCategories;

public sealed class ExpenseCategoryAppService
{
    private readonly IExpenseCategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ExpenseCategoryDomainService _categoryDomain;

    public ExpenseCategoryAppService(
        IExpenseCategoryRepository categories,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ExpenseCategoryDomainService categoryDomain)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _categoryDomain = categoryDomain;
    }

    public async Task<ExpenseCategoryDto> CreateAsync(Guid userId, CreateExpenseCategoryDto dto, CancellationToken cancellationToken = default)
    {
        string name = _categoryDomain.NormalizeName(dto.Name);
        _categoryDomain.ValidateName(name);

        if (await _categories.ExistsNameForUserAsync(userId, name, excludeCategoryId: null, cancellationToken).ConfigureAwait(false))
            throw new ArgumentException("Já existe uma categoria com este nome.");

        ExpenseCategory category = _mapper.Map<ExpenseCategory>(dto);
        category.Id = Guid.NewGuid();
        category.UserId = userId;
        category.Name = name;

        await _categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ExpenseCategory> list = await _categories.ListByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return _mapper.Map<List<ExpenseCategoryDto>>(list);
    }

    public async Task<ExpenseCategoryDto?> GetAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        ExpenseCategory? c = await _categories.GetByIdForUserAsync(categoryId, userId, cancellationToken).ConfigureAwait(false);
        return c is null ? null : _mapper.Map<ExpenseCategoryDto>(c);
    }

    public async Task<ExpenseCategoryDto?> UpdateAsync(
        Guid userId,
        Guid categoryId,
        UpdateExpenseCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        string name = _categoryDomain.NormalizeName(dto.Name);
        _categoryDomain.ValidateName(name);

        ExpenseCategory? category = await _categories.GetTrackedByIdForUserAsync(categoryId, userId, cancellationToken).ConfigureAwait(false);
        if (category is null)
            return null;

        if (await _categories.ExistsNameForUserAsync(userId, name, excludeCategoryId: categoryId, cancellationToken).ConfigureAwait(false))
            throw new ArgumentException("Já existe uma categoria com este nome.");

        category.Name = name;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<bool> SoftDeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        ExpenseCategory? category = await _categories.GetTrackedByIdForUserAsync(categoryId, userId, cancellationToken).ConfigureAwait(false);
        if (category is null)
            return false;

        int inUse = await _categories.CountActiveExpensesByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
        if (inUse > 0)
            throw new ArgumentException("Não é possível eliminar a categoria enquanto existirem gastos associados.");

        category.DeletedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
