namespace SmartStock.Application.DTOs.Customers
{
    public class CreateCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}
