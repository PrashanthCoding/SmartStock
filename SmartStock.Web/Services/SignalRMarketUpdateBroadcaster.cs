using Microsoft.AspNetCore.SignalR;
using SmartStock.Application.DTOs.Market;
using SmartStock.Application.Interfaces.Market;
using SmartStock.Web.Hubs;

namespace SmartStock.Web.Services
{
    public class SignalRMarketUpdateBroadcaster : IMarketUpdateBroadcaster
    {
        private readonly IHubContext<MarketHub> _hubContext;

        public SignalRMarketUpdateBroadcaster(IHubContext<MarketHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task BroadcastMarketUpdateAsync(
            MarketUpdateDto marketUpdate,
            CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(
                "ReceiveMarketUpdate",
                marketUpdate,
                cancellationToken);
        }
    }
}
