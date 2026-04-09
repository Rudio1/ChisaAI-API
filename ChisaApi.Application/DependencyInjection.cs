using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChisaApi.Application.Authentications.Services;
using ChisaApi.Application.Authentications.Settings;
using ChisaApi.Application.ExpenseCategories.Services;
using ChisaApi.Application.Integrations.WhatsApp;
using ChisaApi.Application.Expenses.Profiles;
using ChisaApi.Application.Expenses.Services;
using ChisaApi.Domain.Expenses.ServiceDomain;
using ChisaApi.Domain.Users.ServiceDomain;

namespace ChisaApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));
        services.AddAutoMapper(typeof(ExpenseMappingProfile).Assembly);
        services.AddScoped<ExpenseDomainService>();
        services.AddScoped<ExpenseCategoryDomainService>();
        services.AddScoped<UserRegistrationDomainService>();
        services.AddScoped<AuthAppService>();
        services.AddScoped<ExpenseAppService>();
        services.AddScoped<ExpenseCategoryAppService>();
        services.AddScoped<WhatsAppExpenseAppService>();
        return services;
    }
}
