using AutoMapper;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Requests;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Responses;
using ChisaApi.Domain.Expenses.Entities;

namespace ChisaApi.Application.ExpenseCategories.Profiles;

public sealed class ExpenseCategoryMappingProfile : Profile
{
    public ExpenseCategoryMappingProfile()
    {
        CreateMap<ExpenseCategory, ExpenseCategoryDto>();

        CreateMap<CreateExpenseCategoryDto, ExpenseCategory>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore());

        CreateMap<UpdateExpenseCategoryDto, ExpenseCategory>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore());
    }
}
