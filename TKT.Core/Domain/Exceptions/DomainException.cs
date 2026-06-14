namespace TKT.Core.Domain.Exceptions;

public abstract class DomainException(string code, string? message = null) : Exception(message ?? code)
{
    public string Code { get; } = code;
}
