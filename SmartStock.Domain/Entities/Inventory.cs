using SmartStock.Domain.Common;

namespace SmartStock.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        // SQL Server optimistic concurrency token
        public byte[] RowVersion { get; set; } = [];

        // Navigation property
        public Product Product { get; set; } = null!;

        public ICollection<InventoryMovement> Movements { get; set; }
            = new List<InventoryMovement>();
    }
}
