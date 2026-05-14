using Template.Application.Common;

namespace Template.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void SuccessContainsValueAndNoError()
    {
        var result = Result<string>.Success("created");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("created", result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailureContainsTypedError()
    {
        var error = Error.NotFound("Orders.NotFound", "Order was not found.");

        var result = Result<string>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Orders.NotFound", result.Error.Code);
    }
}
