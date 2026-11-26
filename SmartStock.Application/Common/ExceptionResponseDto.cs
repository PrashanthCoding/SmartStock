namespace SmartStock.Application.Common
{
    public class ExceptionResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
