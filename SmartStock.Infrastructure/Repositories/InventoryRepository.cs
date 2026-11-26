using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Interfaces.Repositories;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public InventoryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get inventory using Product ID
        public async Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Inventories
                .Include(x => x.Product)
                .FirstOrDefaultAsync(
                    x => x.ProductId == productId,
                    cancellationToken);
        }

        // Get inventory using Inventory ID
        public async Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Inventories
                .Include(x => x.Product)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        // Get all inventory records
        public async Task<List<Inventory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Inventories
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderBy(x => x.Product.ProductName)
                .ToListAsync(cancellationToken);
        }

        // Get complete movement history for a product
        public async Task<List<InventoryMovement>> GetMovementsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.InventoryMovements
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Add inventory record
        public async Task AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            await _dbContext.Inventories.AddAsync(
                inventory,
                cancellationToken);
        }

        // Add movement history
        public async Task AddMovementAsync( InventoryMovement movement, CancellationToken cancellationToken = default)
        {
            await _dbContext.InventoryMovements.AddAsync(
                movement,
                cancellationToken);
        }

        // Update inventory
        public void UpdateInventory(Inventory inventory)
        {
            _dbContext.Inventories.Update(inventory);
        }

        // Save changes
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }


        // Process stock movement (Stock In or Stock Out)
        public async Task<Inventory> ProcessStockMovementAsync(
            int productId,
            decimal quantity,
            InventoryMovementType movementType,
            string? remarks,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            try
            {
                var inventory = await _dbContext.Inventories
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(
                        x => x.ProductId == productId,
                        cancellationToken);

                if (inventory == null)
                    throw new KeyNotFoundException(
                        "Inventory record not found for this product.");

                if (!inventory.Product.IsActive)
                    throw new InvalidOperationException(
                        "Stock cannot be changed for an inactive product.");

                var quantityBefore = inventory.Quantity;
                decimal quantityAfter;

                if (movementType == InventoryMovementType.StockIn)
                {
                    quantityAfter = quantityBefore + quantity;
                }
                else if (movementType == InventoryMovementType.StockOut)
                {
                    if (quantity > quantityBefore)
                        throw new InvalidOperationException(
                            "Insufficient stock available.");

                    quantityAfter = quantityBefore - quantity;
                }
                else
                {
                    throw new ArgumentException("Invalid movement type.");
                }

                inventory.Quantity = quantityAfter;

                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    ProductId = inventory.ProductId,
                    MovementType = movementType,
                    Quantity = quantity,
                    QuantityBefore = quantityBefore,
                    QuantityAfter = quantityAfter,
                    Remarks = remarks
                };

                await _dbContext.InventoryMovements.AddAsync(
                    movement, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return inventory;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
