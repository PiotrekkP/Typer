using Typer.Domain.Entities;

namespace Typer.Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void NewProfile_HasZeroPoints()
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal(0, profile.TotalPoints);
    }
}
