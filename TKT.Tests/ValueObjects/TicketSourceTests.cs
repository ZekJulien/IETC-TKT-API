using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class TicketSourceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateOrDefault_WithoutValue_ReturnsWeb(string? raw)
    {
        var source = TicketSource.CreateOrDefault(raw);
        Assert.Equal("web", source.Value);
    }

    [Theory]
    [InlineData("WEB", "web")]
    [InlineData(" Email ", "email")]
    [InlineData("api", "api")]
    public void CreateOrDefault_WithAllowedValue_NormalizesLowercase(string raw, string expected)
    {
        var source = TicketSource.CreateOrDefault(raw);
        Assert.Equal(expected, source.Value);
    }

    [Theory]
    [InlineData("sms")]
    [InlineData("phone")]
    public void CreateOrDefault_WithInvalidValue_ThrowsSourceInvalid(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => TicketSource.CreateOrDefault(raw));
        Assert.Equal(TicketErrors.SourceInvalid, ex.Code);
    }
}
