namespace TKT.Core.Domain.Exceptions;

public sealed class ForbiddenException(string code, string? message = null) : DomainException(code, message);
