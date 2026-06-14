using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_TrimsValueAndNormalizesUppercase()
    {
        var email = Email.Create("  John@Example.com  ");
        Assert.Equal("John@Example.com", email.Value);
        Assert.Equal("JOHN@EXAMPLE.COM", email.Normalized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("noatsign")]
    public void Create_WithInvalidEmail_ThrowsInvalid(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => Email.Create(raw));
        Assert.Equal(AuthErrors.EmailInvalid, ex.Code);
    }
}
