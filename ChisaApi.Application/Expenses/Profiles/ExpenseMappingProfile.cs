using AutoMapper;
using ChisaApi.Application.Expenses.DataTransfer.Requests;
using ChisaApi.Application.Expenses.DataTransfer.Responses;
using ChisaApi.Domain.Expenses.Entities;

namespace ChisaApi.Application.Expenses.Profiles;

public sealed class ExpenseMappingProfile : Profile
{
    public ExpenseMappingProfile()
    {
        CreateMap<Expense, ExpenseDto>();

        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore())
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.Trim()))
            .ForMember(d => d.Note, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Note) ? null : s.Note!.Trim()));

        CreateMap<UpdateExpenseDto, Expense>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore())
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.Trim()))
            .ForMember(d => d.Note, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Note) ? null : s.Note!.Trim()));
    }
}
