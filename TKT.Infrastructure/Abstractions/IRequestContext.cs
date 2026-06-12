namespace TKT.Infrastructure.Abstractions;

public interface IRequestContext
{
    CancellationToken RequestAborted { get; set; }
}