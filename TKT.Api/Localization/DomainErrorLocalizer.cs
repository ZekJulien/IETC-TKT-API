using Microsoft.Extensions.Localization;
using TKT.Api.Account;
using TKT.Api.Auth;
using TKT.Api.Company;
using TKT.Api.Invitation;
using TKT.Api.Ticket;

namespace TKT.Api.Localization;

public sealed class DomainErrorLocalizer(
    IStringLocalizer<AuthErrorMessages> auth,
    IStringLocalizer<AccountErrorMessages> account,
    IStringLocalizer<CompanyErrorMessages> company,
    IStringLocalizer<InvitationErrorMessages> invitation,
    IStringLocalizer<TicketErrorMessages> ticket)
{
    private readonly IReadOnlyDictionary<string, IStringLocalizer> _byDomain = new Dictionary<string, IStringLocalizer>
    {
        ["auth"] = auth,
        ["account"] = account,
        ["company"] = company,
        ["invitation"] = invitation,
        ["ticket"] = ticket,
    };

    public string this[string code]
    {
        get
        {
            var domain = code.Split('.', 2)[0];
            return _byDomain.TryGetValue(domain, out var localizer) ? localizer[code].Value : code;
        }
    }
}
