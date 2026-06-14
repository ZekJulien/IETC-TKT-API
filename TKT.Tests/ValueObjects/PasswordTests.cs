using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_KeepsValue()
    {
        var password = Password.Create("Password1");
        Assert.Equal("Password1", password.Value);
    }

    [Theory]
    [InlineData("Pass1")]
    [InlineData("password1")]
    [InlineData("PASSWORD")]
    public void Create_WithWeakPassword_ThrowsWeak(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => Password.Create(raw));
        Assert.Equal(AuthErrors.PasswordWeak, ex.Code);
    }
}
