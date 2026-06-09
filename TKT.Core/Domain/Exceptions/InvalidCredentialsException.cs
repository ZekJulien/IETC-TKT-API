namespace TKT.Core.Domain.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("Invalid username or password.") { }
}
