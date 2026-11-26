using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common.Models;
using SmartStock.Application.DTOs.Product;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResponse<Product>> SearchAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(x =>
                    x.SKU.Contains(search) ||
                    x.Name.Contains(search) ||
                    x.Category.Contains(search));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var category = request.Category.Trim();

                query = query.Where(x => x.Category == category);
            }

            // Active / Inactive filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            // Sorting
            query = request.SortBy.Trim().ToLowerInvariant() switch
            {
                "sku" => request.SortDescending
                    ? query.OrderByDescending(x => x.SKU)
                    : query.OrderBy(x => x.SKU),

                "name" => request.SortDescending
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name),

                "category" => request.SortDescending
                    ? query.OrderByDescending(x => x.Category)
                    : query.OrderBy(x => x.Category),

                "unitprice" => request.SortDescending
                    ? query.OrderByDescending(x => x.UnitPrice)
                    : query.OrderBy(x => x.UnitPrice),

                "reorderlevel" => request.SortDescending
                    ? query.OrderByDescending(x => x.ReorderLevel)
                    : query.OrderBy(x => x.ReorderLevel),

                "createdatutc" => request.SortDescending
                    ? query.OrderByDescending(x => x.CreatedAtUtc)
                    : query.OrderBy(x => x.CreatedAtUtc),

                _ => request.SortDescending
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name)
            };

            // Total records before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Pagination
            var pageNumber = Math.Max(1, request.PageNumber);

            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<Product>
            {
                Items = products,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
