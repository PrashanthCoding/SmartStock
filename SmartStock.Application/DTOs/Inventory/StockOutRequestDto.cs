using System.ComponentModel.DataAnnotations;

namespace SmartStock.Application.DTOs.Inventory
{
    public class StockOutRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999",
            ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
