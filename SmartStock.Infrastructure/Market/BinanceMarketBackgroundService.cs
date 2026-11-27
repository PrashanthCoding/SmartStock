using System.Globalization;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartStock.Application.DTOs.Market;
using SmartStock.Application.Interfaces.Market;

namespace SmartStock.Infrastructure.Market
{
    public class BinanceMarketBackgroundService : BackgroundService
    {
        private readonly IMarketUpdateBroadcaster _marketBroadcaster;
        private readonly ILogger<BinanceMarketBackgroundService> _logger;

        private const string BinanceWebSocketUrl =
            "wss://stream.binance.com:9443/stream?streams=" +
            "btcusdt@ticker/ethusdt@ticker/bnbusdt@ticker";

        public BinanceMarketBackgroundService(
            IMarketUpdateBroadcaster marketBroadcaster,
            ILogger<BinanceMarketBackgroundService> logger)
        {
            _marketBroadcaster = marketBroadcaster;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var retryDelay = TimeSpan.FromSeconds(5);
            var maxRetryDelay = TimeSpan.FromSeconds(60);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var webSocket = new ClientWebSocket();

                    webSocket.Options.KeepAliveInterval =
                        TimeSpan.FromSeconds(30);

                    _logger.LogInformation(
                        "Connecting to Binance WebSocket...");

                    await webSocket.ConnectAsync(
                        new Uri(BinanceWebSocketUrl),
                        stoppingToken);

                    _logger.LogInformation(
                        "Connected to Binance WebSocket successfully.");

                    retryDelay = TimeSpan.FromSeconds(5);

                    await ReceiveMarketUpdatesAsync(
                        webSocket,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Binance WebSocket connection failed.");
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Reconnecting to Binance in {Delay} seconds...",
                        retryDelay.TotalSeconds);

                    try
                    {
                        await Task.Delay(retryDelay, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    retryDelay = TimeSpan.FromSeconds(
                        Math.Min(retryDelay.TotalSeconds * 2,
                            maxRetryDelay.TotalSeconds));
                }
            }

            _logger.LogInformation(
                "Binance Market Background Service stopped.");
        }

        private async Task ReceiveMarketUpdatesAsync(
            ClientWebSocket webSocket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];

            using var messageStream = new MemoryStream();

            while (webSocket.State == WebSocketState.Open &&
                   !cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new WebSocketException(
                        "Binance WebSocket connection was closed.");
                }

                messageStream.Write(buffer, 0, result.Count);

                if (!result.EndOfMessage)
                {
                    continue;
                }

                var message = messageStream.ToArray();

                messageStream.SetLength(0);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await ProcessMarketMessageAsync(
                        message,
                        cancellationToken);
                }
            }
        }

        private async Task ProcessMarketMessageAsync(
            byte[] message,
            CancellationToken cancellationToken)
        {
            try
            {
                using var document = JsonDocument.Parse(message);

                var root = document.RootElement;

                // Binance combined stream wraps ticker data inside "data".
                if (!root.TryGetProperty(
                    "data", out var tickerData))
                {
                    return;
                }

                var marketUpdate = new MarketUpdateDto
                {
                    Symbol = GetString(tickerData, "s"),

                    Price = GetDecimal(tickerData, "c"),

                    PriceChange = GetDecimal(tickerData, "p"),

                    PriceChangePercent = GetDecimal(tickerData, "P"),

                    HighPrice = GetDecimal(tickerData, "h"),

                    LowPrice = GetDecimal(tickerData, "l"),

                    Volume = GetDecimal(tickerData, "v"),

                    QuoteVolume = GetDecimal(tickerData, "q"),

                    LastUpdated = GetEventTime(tickerData)
                };

                if (string.IsNullOrWhiteSpace(marketUpdate.Symbol))
                {
                    return;
                }

                await _marketBroadcaster.BroadcastMarketUpdateAsync(
                    marketUpdate,
                    cancellationToken);

                _logger.LogDebug(
                    "Market update broadcast: {Symbol} - {Price}",
                    marketUpdate.Symbol,
                    marketUpdate.Price);
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to deserialize Binance ticker message.");
            }
        }

        private static string GetString(
            JsonElement element,
            string propertyName)
        {
            return element.TryGetProperty(
                propertyName, out var property)
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }

        private static decimal GetDecimal(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                propertyName, out var property))
            {
                return 0;
            }

            var value = property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : property.ToString();

            return decimal.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0;
        }

        private static DateTime GetEventTime(JsonElement element)
        {
            if (element.TryGetProperty("E", out var eventTime) &&
                eventTime.TryGetInt64(out var milliseconds))
            {
                return DateTimeOffset
                    .FromUnixTimeMilliseconds(milliseconds)
                    .UtcDateTime;
            }

            return DateTime.UtcNow;
        }
    }
}
