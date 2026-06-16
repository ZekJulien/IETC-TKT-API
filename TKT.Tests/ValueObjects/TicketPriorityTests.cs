using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class TicketPriorityTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateOrDefault_WithoutValue_ReturnsMedium(string? raw)
    {
        var priority = TicketPriority.CreateOrDefault(raw);
        Assert.Equal("medium", priority.Value);
    }

    [Theory]
    [InlineData("LOW", "low")]
    [InlineData("  High ", "high")]
    [InlineData("urgent", "urgent")]
    public void CreateOrDefault_WithAllowedValue_NormalizesLowercase(string raw, string expected)
    {
        var priority = TicketPriority.CreateOrDefault(raw);
        Assert.Equal(expected, priority.Value);
    }

    [Theory]
    [InlineData("critical")]
    [InlineData("none")]
    public void CreateOrDefault_WithInvalidValue_ThrowsPriorityInvalid(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => TicketPriority.CreateOrDefault(raw));
        Assert.Equal(TicketErrors.PriorityInvalid, ex.Code);
    }
}
