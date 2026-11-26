namespace SmartStock.Application.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
