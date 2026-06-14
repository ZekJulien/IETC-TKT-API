namespace TKT.Core.Domain.Exceptions;

public sealed class NotFoundException(string code, string? message = null) : DomainException(code, message);
