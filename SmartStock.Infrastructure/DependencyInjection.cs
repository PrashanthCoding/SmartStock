
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Authentication;
using SmartStock.Infrastructure.Identity.Services;
using SmartStock.Infrastructure.Persistence.Context;
using SmartStock.Infrastructure.Repositories;
using SmartStock.Infrastructure.Services;

namespace SmartStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ======================================
        // DATABASE
        // ======================================
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is missing.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        // ======================================
        // PASSWORD HASHING
        // ======================================
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // ======================================
        // REPOSITORIES
        // ======================================
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        services.AddScoped<IInventoryService, InventoryService>();

        // ======================================
        // JWT
        // ======================================
        services.Configure<JwtSettings>( configuration.GetSection("JwtSettings"));

        services.AddScoped<JwtTokenGenerator>();

        // ======================================
        // AUTHENTICATION
        // ======================================
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
