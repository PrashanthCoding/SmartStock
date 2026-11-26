namespace SmartStock.Application.DTOs.CustomerOrders
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }
    }
}
