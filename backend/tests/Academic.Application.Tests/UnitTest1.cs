using Academic.Application.Common;

namespace Academic.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldContainValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ShouldContainError()
    {
        var result = Result<int>.Failure("business_rule", "duplicate request");

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("business_rule", result.Error!.Code);
    }
}
