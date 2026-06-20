using TKT.Core.Abstractions;
using TKT.Core.Domain;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Onboarding.CreateCompany;

public sealed class CreateCompanyUseCase(
    ICompanyProvisioningGateway companyGateway,
    ICompanyMemberProvisioningGateway memberGateway,
    ITokenService tokenService,
    IRefreshTokenIssuer refreshTokenIssuer) : ICreateCompanyUseCase
{
    private readonly ICompanyProvisioningGateway _companyGateway = companyGateway;
    private readonly ICompanyMemberProvisioningGateway _memberGateway = memberGateway;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = refreshTokenIssuer;

    public async Task<CreateCompanyResult> ExecuteAsync(CreateCompanyInput input)
    {
        var name = CompanyName.Create(input.CompanyName);
        var slug = CompanySlug.Create(input.CompanySlug);

        if (await _companyGateway.ExistsByNameOrSlugAsync(name.Value, slug.Value))
            throw new ConflictException(CompanyErrors.AlreadyExists);

        var company = new Company
        {
            CompanyId = Guid.CreateVersion7(),
            Name = name.Value,
            Slug = slug.Value,
        };
        await _companyGateway.AddCompanyAsync(company);
        await _companyGateway.AddFreeSubscriptionAsync(company.CompanyId);

        var owner = new CompanyMember
        {
            MembershipId = Guid.CreateVersion7(),
            CompanyId = company.CompanyId,
            AccountId = input.AccountId,
            Role = CompanyRoles.Owner,
            JoinedAt = DateTimeOffset.UtcNow,
        };
        await _memberGateway.AddMemberAsync(owner);

        var accessToken = _tokenService.GenerateAccessToken(input.AccountId, input.Email, company.CompanyId, owner.Role);
        var refreshToken = await _refreshTokenIssuer.IssueAsync(input.AccountId, company.CompanyId);

        return new CreateCompanyResult(company.CompanyId, company.Name, company.Slug, owner.Role, accessToken, refreshToken);
    }
}
