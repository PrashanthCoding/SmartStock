using SmartStock.Application.DTOs.Reports;

namespace SmartStock.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(
            CancellationToken cancellationToken = default);

        Task<List<OrderStatusSummaryDto>> GetOrderStatusSummaryAsync(
            CancellationToken cancellationToken = default);

        Task<List<MonthlyOrderSummaryDto>> GetMonthlyOrderSummaryAsync(
            CancellationToken cancellationToken = default);

        Task<List<LowStockProductDto>> GetLowStockProductsAsync(
            CancellationToken cancellationToken = default);

        Task<List<RecentMovementDto>> GetRecentMovementsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);
    }
}
