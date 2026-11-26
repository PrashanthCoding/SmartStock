namespace SmartStock.Application.DTOs.Inventory
{
    public class InventoryMovementResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal QuantityBefore { get; set; }
        public decimal QuantityAfter { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
