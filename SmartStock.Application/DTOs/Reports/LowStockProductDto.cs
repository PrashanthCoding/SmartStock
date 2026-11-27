namespace SmartStock.Application.DTOs.Reports
{
    public class LowStockProductDto
    {
        public int ProductId { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal AvailableQuantity { get; set; }

        public decimal ReorderLevel { get; set; }
    }
}
