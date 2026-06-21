using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

public class RequestContext : IRequestContext
{
    public CancellationToken RequestAborted { get; set; }
}