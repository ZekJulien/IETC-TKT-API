using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Auth.Register;

public sealed class RegisterAccountUseCase(IAccountGateway accountGateway, IProfileGateway profileGateway,
    IPasswordHasher passwordHasher, IEmailSender emailSender, ITokenService tokenService) : IRegisterAccountUseCase
{
    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly IProfileGateway _profileGateway = profileGateway;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ITokenService _tokenService = tokenService;

    public async Task ExecuteAsync(RegisterInput req)
    {
        var email = Email.Create(req.Email);
        var password = Password.Create(req.Password);
        if (req.Password != req.ConfirmPassword) throw new ValidationException(AuthErrors.PasswordMismatch);

        var firstName = req.FirstName?.Trim() ?? string.Empty;
        var lastName = req.LastName?.Trim() ?? string.Empty;
        if (firstName.Length == 0 || lastName.Length == 0) throw new ValidationException(AuthErrors.NameRequired);

        if (await _accountGateway.ExistByEmailAsync(email.Normalized)) throw new ConflictException(AuthErrors.EmailAlreadyUsed);

        var newAccount = new Account
        {
            AccountId = Guid.CreateVersion7(),
            Email = email.Value,
            NormalizedEmail = email.Normalized,
            PasswordHash = _passwordHasher.Hash(password.Value),
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        await _accountGateway.AddAccount(newAccount);

        await _profileGateway.AddAsync(new UserProfile
        {
            AccountId = newAccount.AccountId,
            FirstName = firstName,
            LastName = lastName,
        });

        var token = _tokenService.GenerateEmailConfirmationToken(newAccount);
        await _emailSender.SendAsync(email.Value, "TKT - Inscription", $"activation : {token}");
    }
}
