namespace TKT.Core.Domain.Exceptions;

public sealed class ValidationException(string code, string? message = null) : DomainException(code, message);
