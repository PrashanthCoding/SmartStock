namespace SmartStock.Application.DTOs.CustomerOrders
{
    public class OrderItemResponseDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}
