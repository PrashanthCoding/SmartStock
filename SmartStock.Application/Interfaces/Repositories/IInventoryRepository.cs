using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;

namespace SmartStock.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);

        Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<Inventory>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<InventoryMovement>> GetMovementsByProductIdAsync(int productId, CancellationToken cancellationToken = default);

        Task AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken = default);

        Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default);

        void UpdateInventory(Inventory inventory);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<Inventory> ProcessStockMovementAsync(
        int productId,
        decimal quantity,
        InventoryMovementType movementType,
        string? remarks,
        CancellationToken cancellationToken = default);
    }
}
