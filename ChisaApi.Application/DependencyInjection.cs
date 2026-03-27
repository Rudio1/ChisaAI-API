using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChisaApi.Application.Auth;
using ChisaApi.Application.Expenses;
using ChisaApi.Application.Expenses.Profiles;
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
        services.AddScoped<UserRegistrationDomainService>();
        services.AddScoped<AuthAppService>();
        services.AddScoped<ExpenseAppService>();
        return services;
    }
}
