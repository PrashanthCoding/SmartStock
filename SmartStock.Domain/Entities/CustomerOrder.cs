    using SmartStock.Domain.Common;
    using SmartStock.Domain.Enums;

    namespace SmartStock.Domain.Entities
    {
        public class CustomerOrder : BaseEntity
        {
            public string OrderNumber { get; set; } = string.Empty;

            public int CustomerId { get; set; }

            public DateTime OrderDateUtc { get; set; }

            public OrderStatus Status { get; set; } = OrderStatus.Confirmed;

            public decimal TotalAmount { get; set; }

            public Customer Customer { get; set; } = null!;

            public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        }
    }
