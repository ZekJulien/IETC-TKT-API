namespace TKT.Core.Domain.Errors;

public static class CompanyErrors
{
    public const string NameRequired = "company.name.required";
    public const string NameTooLong = "company.name.too_long";
    public const string SlugRequired = "company.slug.required";
    public const string SlugTooLong = "company.slug.too_long";
    public const string SlugInvalid = "company.slug.invalid";
    public const string AlreadyExists = "company.already_exists";
}
