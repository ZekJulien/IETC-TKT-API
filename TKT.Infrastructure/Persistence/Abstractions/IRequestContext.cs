namespace TKT.Infrastructure.Persistence.Abstractions;

public interface IRequestContext
{
    CancellationToken RequestAborted { get; set; }
}
