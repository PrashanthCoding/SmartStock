using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Infrastructure.Authentication;
using SmartStock.Infrastructure.Identity;
using SmartStock.Infrastructure.Identity.Services;
using SmartStock.Infrastructure.Persistence.Context;
using SmartStock.Infrastructure.Repositories;

namespace SmartStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is missing.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        // ASP.NET Core Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IProductRepository, ProductRepository>();

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<JwtTokenGenerator>();

        // Authentication
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
