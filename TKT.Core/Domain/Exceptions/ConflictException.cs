namespace TKT.Core.Domain.Exceptions;

public sealed class ConflictException(string code, string? message = null) : DomainException(code, message);
