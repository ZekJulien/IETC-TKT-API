using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class TicketTitleTests
{
    [Fact]
    public void Create_WithValidTitle_TrimsValue()
    {
        var title = TicketTitle.Create("  Imprimante en panne  ");
        Assert.Equal("Imprimante en panne", title.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("  a ")]
    public void Create_WithTooShortTitle_ThrowsTitleInvalid(string raw)
    {
        var ex = Assert.Throws<ValidationException>(() => TicketTitle.Create(raw));
        Assert.Equal(TicketErrors.TitleInvalid, ex.Code);
    }

    [Fact]
    public void Create_WithTooLongTitle_ThrowsTitleTooLong()
    {
        var raw = new string('x', 256);
        var ex = Assert.Throws<ValidationException>(() => TicketTitle.Create(raw));
        Assert.Equal(TicketErrors.TitleTooLong, ex.Code);
    }
}
