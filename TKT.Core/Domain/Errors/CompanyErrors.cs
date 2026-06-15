namespace TKT.Core.Domain.Errors;

public static class CompanyErrors
{
    public const string NameRequired = "company.name.required";
    public const string NameTooLong = "company.name.too_long";
    public const string SlugRequired = "company.slug.required";
    public const string SlugTooLong = "company.slug.too_long";
    public const string SlugInvalid = "company.slug.invalid";
    public const string AlreadyExists = "company.already_exists";
    public const string Forbidden = "company.forbidden";
    public const string MemberNotFound = "company.member_not_found";
    public const string LastOwner = "company.last_owner";
    public const string MemberQuotaExceeded = "company.member_quota_exceeded";
}
