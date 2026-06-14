using TKT.Core.Domain.Errors;

namespace TKT.Core.Domain.Exceptions;

public sealed class InvalidCredentialsException(string? message = null)
    : DomainException(AuthErrors.InvalidCredentials, message);
