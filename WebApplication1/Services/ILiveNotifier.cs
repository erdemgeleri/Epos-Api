namespace WebApplication1.Services;

public interface ILiveNotifier
{
    Task OrderCreatedAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default);
    Task OrderCancelledAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default);
    Task ProductChangedAsync(Guid businessId, object payload, CancellationToken cancellationToken = default);
    Task ChatMessageAsync(Guid businessId, Guid customerUserId, object payload, CancellationToken cancellationToken = default);
    Task UserChangedAsync(object payload, CancellationToken cancellationToken = default);
    Task BusinessChangedAsync(Guid? businessId, object payload, CancellationToken cancellationToken = default);
}
