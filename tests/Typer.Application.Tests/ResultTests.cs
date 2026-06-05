using Typer.Application.Common.Models;

namespace Typer.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSucceededResult()
    {
        var result = Result.Success();

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ReturnsErrorMessage()
    {
        var result = Result.Failure("Błąd");

        Assert.False(result.Succeeded);
        Assert.Equal("Błąd", result.Error);
    }
}
