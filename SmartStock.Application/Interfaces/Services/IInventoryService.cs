using SmartStock.Application.DTOs.Inventory;

namespace SmartStock.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> StockInAsync(
            StockInRequestDto request,
            CancellationToken cancellationToken = default);

        Task<InventoryResponseDto> StockOutAsync(
            StockOutRequestDto request,
            CancellationToken cancellationToken = default);

        Task<InventoryResponseDto?> GetByProductIdAsync(
            int productId,
            CancellationToken cancellationToken = default);

        Task<List<InventoryResponseDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<List<InventoryResponseDto>> GetLowStockAsync(
            CancellationToken cancellationToken = default);

        Task<List<InventoryMovementResponseDto>> GetMovementsByProductIdAsync(
            int productId,
            CancellationToken cancellationToken = default);
    }
}
