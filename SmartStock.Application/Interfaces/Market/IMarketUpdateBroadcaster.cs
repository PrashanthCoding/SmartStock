using SmartStock.Application.DTOs.Market;

namespace SmartStock.Application.Interfaces.Market
{
    public interface IMarketUpdateBroadcaster
    {
        Task BroadcastMarketUpdateAsync(
            MarketUpdateDto marketUpdate,
            CancellationToken cancellationToken = default);
    }
}
