namespace SmartStock.Application.DTOs.Market
{
    public class MarketUpdateDto
    {
        public string Symbol { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal PriceChange { get; set; }

        public decimal PriceChangePercent { get; set; }

        public decimal HighPrice { get; set; }

        public decimal LowPrice { get; set; }

        public decimal Volume { get; set; }

        public decimal QuoteVolume { get; set; }

        public DateTime EventTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
