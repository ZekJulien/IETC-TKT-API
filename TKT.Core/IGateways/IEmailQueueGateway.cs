namespace TKT.Core.IGateways;

public interface IEmailQueueGateway
{
    Task EnqueueAsync(QueuedEmail email);
}
