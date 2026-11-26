using Microsoft.Extensions.DependencyInjection;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Application.Services;

namespace SmartStock.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Product Service
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
