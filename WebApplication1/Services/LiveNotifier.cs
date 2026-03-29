using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;

namespace WebApplication1.Services;

/// <summary>Scoped: IHubContext çözümü için uygun ömür.</summary>
public sealed class LiveNotifier : ILiveNotifier
{
    private readonly IHubContext<LiveHub> _hubContext;

    public LiveNotifier(IHubContext<LiveHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task OrderCreatedAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("orderCreated", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.BusinessGroup(businessId)).SendAsync("orderCreated", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.CustomerGroup(customerUserId)).SendAsync("orderCreated", payload, cancellationToken);
    }

    public async Task OrderCancelledAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("orderCancelled", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.BusinessGroup(businessId)).SendAsync("orderCancelled", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.CustomerGroup(customerUserId)).SendAsync("orderCancelled", payload, cancellationToken);
    }

    public async Task ProductChangedAsync(Guid businessId, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("productChanged", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.BusinessGroup(businessId)).SendAsync("productChanged", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.CustomerCatalogGroup).SendAsync("productChanged", payload, cancellationToken);
    }

    public async Task ChatMessageAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("chatMessage", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.BusinessGroup(businessId)).SendAsync("chatMessage", payload, cancellationToken);
        await _hubContext.Clients.Group(LiveHub.CustomerGroup(customerUserId)).SendAsync("chatMessage", payload, cancellationToken);
    }

    public Task UserChangedAsync(object payload, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("userChanged", payload, cancellationToken);

    public async Task BusinessChangedAsync(Guid? businessId, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _hubContext.Clients.Group(LiveHub.AdminGroup).SendAsync("businessChanged", payload, cancellationToken);
        if (businessId.HasValue)
        {
            await _hubContext.Clients.Group(LiveHub.BusinessGroup(businessId.Value)).SendAsync("businessChanged", payload, cancellationToken);
        }
    }
}
