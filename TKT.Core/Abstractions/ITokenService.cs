using TKT.Core.Domain.Entities;

namespace TKT.Core.Abstractions;

public interface ITokenService
{
    string GenerateEmailConfirmationToken(Account account);

    EmailConfirmationToken ValidateEmailConfirmationToken(string token);

    string GenerateAccessToken(Guid accountId, string email);
}
