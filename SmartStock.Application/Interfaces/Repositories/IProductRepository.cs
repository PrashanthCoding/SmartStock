using SmartStock.Application.Common.Models;
using SmartStock.Application.DTOs.Product;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<PagedResponse<Product>> SearchAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
