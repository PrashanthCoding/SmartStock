using SmartStock.Application.Common.Models;
using SmartStock.Application.DTOs.Product;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // GET ALL PRODUCTS
    public async Task<PagedResponse<ProductResponseDto>> GetProductsAsync(
        ProductSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productRepository.SearchAsync(
            request,
            cancellationToken);

        return new PagedResponse<ProductResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    // GET PRODUCT BY ID
    public async Task<ProductResponseDto?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        return product is null ? null : MapToResponse(product);
    }

    // CREATE PRODUCT
    public async Task<ProductResponseDto> CreateProductAsync(
        CreateProductDto request,
        CancellationToken cancellationToken = default)
    {
        var sku = request.SKU.Trim().ToUpperInvariant();

        var exists = await _productRepository.ExistsAsync(
            x => x.SKU == sku,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Product SKU '{sku}' already exists.");
        }

        var product = new Product
        {
            SKU = request.SKU.Trim().ToUpperInvariant(),
            ProductName = request.ProductName.Trim(),
            Category = request.Category.Trim(),
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            ReorderLevel = request.ReorderLevel,
            IsActive = true,

            // Automatically create initial inventory
            Inventory = new Inventory
            {
                Quantity = 0
            }
        };

        await _productRepository.AddAsync(product, cancellationToken);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    // UPDATE PRODUCT
    public async Task<ProductResponseDto?> UpdateProductAsync(
        int id,
        UpdateProductDto request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null || !product.IsActive)
        {
            return null;
        }

        var sku = request.SKU.Trim().ToUpperInvariant();

        var duplicateSku = await _productRepository.ExistsAsync(
            x => x.SKU == sku && x.Id != id,
            cancellationToken);

        if (duplicateSku)
        {
            throw new InvalidOperationException(
                $"Product SKU '{sku}' already exists.");
        }

        product.SKU = sku;
        product.ProductName = request.ProductName.Trim();
        product.Category = request.Category.Trim();
        product.Description = request.Description?.Trim();
        product.UnitPrice = request.UnitPrice;
        product.ReorderLevel = request.ReorderLevel;

        _productRepository.Update(product);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    // DEACTIVATE PRODUCT
    public async Task<bool> DeactivateProductAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null || !product.IsActive)
        {
            return false;
        }

        product.IsActive = false;

        _productRepository.Update(product);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ENTITY TO DTO MAPPING
    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            ProductName = product.ProductName,
            Category = product.Category,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        };
    }
}
