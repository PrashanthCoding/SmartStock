namespace SmartStock.Application.DTOs.Product
{
    public class ProductSearchRequestDto
    {
        public string? Search { get; set; }

        public string? Category { get; set; }

        public bool? IsActive { get; set; }

        public string SortBy { get; set; } = "Name";

        public bool SortDescending { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
