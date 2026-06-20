namespace TKT.Infrastructure.Repositories;

internal static class CompanyMemberSql
{
    public const string Insert = """
                                 INSERT INTO company_members (membership_id, company_id, account_id, role,
                                                              invited_by, department, job_title, joined_at)
                                 VALUES (@MembershipId, @CompanyId, @AccountId, @Role,
                                         @InvitedBy, @Department, @JobTitle, @JoinedAt);
                                 """;

    public const string MemberExists =
        "SELECT EXISTS(SELECT 1 FROM company_members WHERE company_id = @CompanyId AND account_id = @AccountId);";
}
