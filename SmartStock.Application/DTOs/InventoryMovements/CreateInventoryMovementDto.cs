using SmartStock.Domain.Enums;

namespace SmartStock.Application.DTOs.InventoryMovements
{
    public class CreateInventoryMovementDto
    {
        public int ProductId { get; set; }

        public InventoryMovementType MovementType { get; set; }

        public decimal Quantity { get; set; }

        public string? Remarks { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}