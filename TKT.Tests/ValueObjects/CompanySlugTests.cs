using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class CompanySlugTests
{
    [Theory]
    [InlineData("my-company", "my-company")]
    [InlineData("My-Company", "my-company")]
    [InlineData("  acme  ", "acme")]
    [InlineData("abc123", "abc123")]
    public void Create_WithValidSlug_NormalizesToTrimmedLowercase(string raw, string expected)
    {
        var slug = CompanySlug.Create(raw);
        Assert.Equal(expected, slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySlug_ThrowsRequired(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => CompanySlug.Create(raw));
        Assert.Equal(CompanyErrors.SlugRequired, ex.Code);
    }

    [Fact]
    public void Create_WithTooLongSlug_ThrowsTooLong()
    {
        var ex = Assert.Throws<ValidationException>(() => CompanySlug.Create(new string('a', 101)));
        Assert.Equal(CompanyErrors.SlugTooLong, ex.Code);
    }

    [Theory]
    [InlineData("-bad")]
    [InlineData("bad-")]
    [InlineData("a b")]
    [InlineData("a")]
    [InlineData("bad_slug")]
    public void Create_WithInvalidPattern_ThrowsInvalid(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => CompanySlug.Create(raw));
        Assert.Equal(CompanyErrors.SlugInvalid, ex.Code);
    }
}
