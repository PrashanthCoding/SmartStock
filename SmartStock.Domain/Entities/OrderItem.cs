using SmartStock.Domain.Common;

namespace SmartStock.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int CustomerOrderId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }

        public CustomerOrder CustomerOrder { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
