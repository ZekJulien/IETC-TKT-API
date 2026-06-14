using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class CompanyNameTests
{
    [Theory]
    [InlineData("Acme", "Acme")]
    [InlineData("  Acme Corp  ", "Acme Corp")]
    public void Create_WithValidName_Trims(string raw, string expected)
    {
        var name = CompanyName.Create(raw);
        Assert.Equal(expected, name.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ThrowsRequired(string? raw)
    {
        var ex = Assert.Throws<ValidationException>(() => CompanyName.Create(raw));
        Assert.Equal(CompanyErrors.NameRequired, ex.Code);
    }

    [Fact]
    public void Create_WithTooLongName_ThrowsTooLong()
    {
        var ex = Assert.Throws<ValidationException>(() => CompanyName.Create(new string('a', 256)));
        Assert.Equal(CompanyErrors.NameTooLong, ex.Code);
    }
}
