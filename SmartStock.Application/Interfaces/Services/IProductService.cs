using SmartStock.Application.Common.Models;
using SmartStock.Application.DTOs.Product;

namespace SmartStock.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<PagedResponse<ProductResponseDto>> GetProductsAsync(ProductSearchRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ProductResponseDto?> GetProductByIdAsync(int id,
            CancellationToken cancellationToken = default);

        Task<ProductResponseDto> CreateProductAsync(CreateProductDto request,
            CancellationToken cancellationToken = default);

        Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeactivateProductAsync(int id,
            CancellationToken cancellationToken = default);
    }
}
