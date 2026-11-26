using SmartStock.Application.DTOs.Inventory;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;

namespace SmartStock.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // STOCK IN
        public async Task<InventoryResponseDto> StockInAsync(
            StockInRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var inventory = await _inventoryRepository.ProcessStockMovementAsync(
                request.ProductId,
                request.Quantity,
                InventoryMovementType.StockIn,
                request.Remarks,
                cancellationToken);

            return MapInventory(inventory);
        }

        // STOCK OUT
        public async Task<InventoryResponseDto> StockOutAsync(
            StockOutRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var inventory = await _inventoryRepository.ProcessStockMovementAsync(
                request.ProductId,
                request.Quantity,
                InventoryMovementType.StockOut,
                request.Remarks,
                cancellationToken);

            return MapInventory(inventory);
        }

        // GET INVENTORY BY PRODUCT
        public async Task<InventoryResponseDto?> GetByProductIdAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(
                productId, cancellationToken);

            return inventory == null ? null : MapInventory(inventory);
        }

        // GET ALL INVENTORY
        public async Task<List<InventoryResponseDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var inventories = await _inventoryRepository.GetAllAsync(
                cancellationToken);

            return inventories
                .Where(x => x.Product.IsActive)
                .Select(MapInventory)
                .ToList();
        }

        // GET LOW STOCK PRODUCTS
        public async Task<List<InventoryResponseDto>> GetLowStockAsync(
            CancellationToken cancellationToken = default)
        {
            var inventories = await _inventoryRepository.GetAllAsync(
                cancellationToken);

            return inventories
                .Where(x =>
                    x.Product.IsActive &&
                    x.Quantity <= x.Product.ReorderLevel)
                .Select(MapInventory)
                .ToList();
        }

        // GET MOVEMENT HISTORY
        public async Task<List<InventoryMovementResponseDto>>
            GetMovementsByProductIdAsync(
                int productId,
                CancellationToken cancellationToken = default)
        {
            var movements =
                await _inventoryRepository.GetMovementsByProductIdAsync(
                    productId, cancellationToken);

            return movements.Select(x => new InventoryMovementResponseDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                SKU = x.Product.SKU,
                ProductName = x.Product.ProductName,
                MovementType = x.MovementType.ToString(),
                Quantity = x.Quantity,
                QuantityBefore = x.QuantityBefore,
                QuantityAfter = x.QuantityAfter,
                Remarks = x.Remarks,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList();
        }

        // INVENTORY DTO MAPPING
        private static InventoryResponseDto MapInventory(Inventory inventory)
        {
            return new InventoryResponseDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                SKU = inventory.Product.SKU,
                ProductName = inventory.Product.ProductName,
                Quantity = inventory.Quantity,
                ReorderLevel = inventory.Product.ReorderLevel,
                IsLowStock = inventory.Quantity <= inventory.Product.ReorderLevel,
                IsOutOfStock = inventory.Quantity <= 0
            };
        }
    }
}
