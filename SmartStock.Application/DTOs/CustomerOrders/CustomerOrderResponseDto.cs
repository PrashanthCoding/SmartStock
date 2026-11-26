using SmartStock.Domain.Enums;

namespace SmartStock.Application.DTOs.CustomerOrders
{
    public class CustomerOrderResponseDto
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDateUtc { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
