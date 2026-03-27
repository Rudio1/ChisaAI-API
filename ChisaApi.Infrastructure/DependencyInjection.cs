using ChisaApi.Application.Abstractions;
using ChisaApi.Application.Auth;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Domain.Users;
using ChisaApi.Domain.Users.Entities;
using ChisaApi.Infrastructure.Auth;
using ChisaApi.Infrastructure.Data;
using ChisaApi.Infrastructure.Expenses.Repositories;
using ChisaApi.Infrastructure.Users.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChisaApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();

        return services;
    }
}
