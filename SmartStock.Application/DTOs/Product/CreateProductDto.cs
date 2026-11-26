using System.ComponentModel.DataAnnotations;

namespace SmartStock.Application.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
    }
}
