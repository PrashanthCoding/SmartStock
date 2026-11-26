using SmartStock.Domain.Common;

namespace SmartStock.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
    }
}
