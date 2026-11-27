using Microsoft.EntityFrameworkCore;
using SmartStock.Application.DTOs.Reports;
using SmartStock.Application.Interfaces.Services;
using SmartStock.Infrastructure.Persistence.Context;
using System.Globalization;

namespace SmartStock.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DASHBOARD SUMMARY
        // ==========================================
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var activeProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(x => x.IsActive, cancellationToken);

            var inventoryValue = await _context.Inventories
                .AsNoTracking()
                .Where(x => x.Product.IsActive)
                .SumAsync(
                    x => (decimal?)x.Quantity * x.Product.UnitPrice,
                    cancellationToken) ?? 0m;

            var totalOrders = await _context.CustomerOrders
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var lowStockCount = await _context.Inventories
                .AsNoTracking()
                .CountAsync(
                    x => x.Product.IsActive &&
                         x.Quantity <= x.Product.ReorderLevel,
                    cancellationToken);

            var result = new DashboardSummaryDto
            {
                TotalActiveProducts = activeProducts,
                TotalInventoryValue = inventoryValue,
                TotalOrders = totalOrders,
                LowStockCount = lowStockCount,

                OrderStatusSummary =
                    await GetOrderStatusSummaryAsync(cancellationToken),

                MonthlyOrderSummary =
                    await GetMonthlyOrderSummaryAsync(cancellationToken),

                LowStockProducts =
                    await GetLowStockProductsAsync(cancellationToken),

                RecentMovements =
                    await GetRecentMovementsAsync(10, cancellationToken)
            };

            return result;
        }

        // ==========================================
        // ORDER STATUS SUMMARY
        // ==========================================
        public async Task<List<OrderStatusSummaryDto>>
            GetOrderStatusSummaryAsync(
                CancellationToken cancellationToken = default)
        {
            var data = await _context.CustomerOrders
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return data
                .Select(x => new OrderStatusSummaryDto
                {
                    Status = x.Status.ToString(),
                    Count = x.Count
                })
                .OrderBy(x => x.Status)
                .ToList();
        }

        // ==========================================
        // MONTHLY ORDER SUMMARY
        // ==========================================
        public async Task<List<MonthlyOrderSummaryDto>>
            GetMonthlyOrderSummaryAsync(
                CancellationToken cancellationToken = default)
        {
            var currentYear = DateTime.UtcNow.Year;

            var data = await _context.CustomerOrders
                .AsNoTracking()
                .Where(x => x.OrderDateUtc.Year == currentYear)
                .GroupBy(x => new
                {
                    x.OrderDateUtc.Year,
                    x.OrderDateUtc.Month
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalOrders = g.Count(),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync(cancellationToken);

            return data
                .Select(x => new MonthlyOrderSummaryDto
                {
                    Year = x.Year,
                    Month = x.Month,
                    MonthName = CultureInfo.InvariantCulture
                        .DateTimeFormat.GetMonthName(x.Month),
                    TotalOrders = x.TotalOrders,
                    TotalAmount = x.TotalAmount
                })
                .OrderBy(x => x.Month)
                .ToList();
        }

        // ==========================================
        // LOW STOCK PRODUCTS
        // ==========================================
        public async Task<List<LowStockProductDto>>
            GetLowStockProductsAsync(
                CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(x =>
                    x.Product.IsActive &&
                    x.Quantity <= x.Product.ReorderLevel)
                .OrderBy(x => x.Quantity)
                .Select(x => new LowStockProductDto
                {
                    ProductId = x.ProductId,
                    SKU = x.Product.SKU,
                    ProductName = x.Product.ProductName,
                    Category = x.Product.Category,
                    AvailableQuantity = x.Quantity,
                    ReorderLevel = x.Product.ReorderLevel
                })
                .ToListAsync(cancellationToken);
        }

        // ==========================================
        // RECENT INVENTORY MOVEMENTS
        // ==========================================
        public async Task<List<RecentMovementDto>>
            GetRecentMovementsAsync(
                int count = 10,
                CancellationToken cancellationToken = default)
        {
            count = Math.Clamp(count, 1, 100);

            return await _context.InventoryMovements
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(count)
                .Select(x => new RecentMovementDto
                {
                    MovementId = x.Id,
                    ProductId = x.ProductId,
                    SKU = x.Product.SKU,
                    ProductName = x.Product.ProductName,
                    MovementType = x.MovementType.ToString(),
                    Quantity = x.Quantity,
                    QuantityBefore = x.QuantityBefore,
                    QuantityAfter = x.QuantityAfter,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }
    }
}
