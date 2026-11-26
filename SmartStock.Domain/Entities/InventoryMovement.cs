using SmartStock.Domain.Common;
using SmartStock.Domain.Enums;

namespace SmartStock.Domain.Entities
{
    public class InventoryMovement : BaseEntity
    {
        public int InventoryId { get; set; }

        public int ProductId { get; set; }

        public InventoryMovementType MovementType { get; set; }

        // Always store a positive movement quantity
        public decimal Quantity { get; set; }

        // Stock before the transaction
        public decimal QuantityBefore { get; set; }

        // Stock after the transaction
        public decimal QuantityAfter { get; set; }

        public string? Remarks { get; set; }

        // Navigation properties
        public Inventory Inventory { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
