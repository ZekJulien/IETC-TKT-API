namespace TKT.Core.UseCases.Auth.Register;

public sealed record RegisterInput(string Email, string Password, string ConfirmPassword);
