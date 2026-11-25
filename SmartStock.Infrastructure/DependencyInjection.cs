using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Infrastructure.Persistence.Context;
using SmartStock.Infrastructure.Repositories;

namespace SmartStock.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Database connection string is missing.");
            }

            // SQL Server
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            // Generic Repository
            services.AddScoped( typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }
    }
}
