namespace SmartStock.Application.DTOs.Customers
{
    public class UpdateCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }
    }
}
