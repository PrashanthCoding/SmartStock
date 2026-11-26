namespace SmartStock.Application.DTOs.CustomerOrders
{
    public class CreateCustomerOrderDto
    {
        public int CustomerId { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}