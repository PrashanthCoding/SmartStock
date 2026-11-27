namespace SmartStock.Application.DTOs.Reports
{
    public class DashboardSummaryDto
    {
        public int TotalActiveProducts { get; set; }

        public decimal TotalInventoryValue { get; set; }

        public int TotalOrders { get; set; }

        public int LowStockCount { get; set; }

        public List<OrderStatusSummaryDto> OrderStatusSummary { get; set; } = new();

        public List<MonthlyOrderSummaryDto> MonthlyOrderSummary { get; set; } = new();

        public List<LowStockProductDto> LowStockProducts { get; set; } = new();

        public List<RecentMovementDto> RecentMovements { get; set; } = new();
    }
}
