using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class EmailQueueGateway(IEmailQueueRepository repository) : IEmailQueueGateway
{
    private readonly IEmailQueueRepository _repository = repository;

    public Task EnqueueAsync(QueuedEmail email)
        => _repository.InsertAsync(email.ToRow(Guid.CreateVersion7()));
}
