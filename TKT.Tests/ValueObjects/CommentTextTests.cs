using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using Xunit;

namespace TKT.Tests.ValueObjects;

public class CommentTextTests
{
    [Fact]
    public void Create_WithValidText_TrimsValue()
    {
        var text = CommentText.Create("  Bonjour  ");
        Assert.Equal("Bonjour", text.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyText_ThrowsTextInvalid(string? raw)
    {
        var ex = Assert.Throws<ValidationException>(() => CommentText.Create(raw));
        Assert.Equal(CommentErrors.TextInvalid, ex.Code);
    }

    [Fact]
    public void Create_WithTooLongText_ThrowsTextInvalid()
    {
        var raw = new string('x', 10001);
        var ex = Assert.Throws<ValidationException>(() => CommentText.Create(raw));
        Assert.Equal(CommentErrors.TextInvalid, ex.Code);
    }
}
