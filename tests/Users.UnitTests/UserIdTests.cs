using Users.Domain.Common;
using Users.Domain.UserAggregate;

namespace Users.UnitTests;

public class UserIdTests
{
    [Fact]
    public void New_CalledTwice_ReturnsDistinctNonEmptyIds()
    {
        var first = UserId.New();
        var second = UserId.New();

        Assert.NotEqual(Guid.Empty, first.Value);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void From_EmptyGuid_ThrowsDomainException() => Assert.Throws<DomainException>(() => UserId.From(Guid.Empty));

    [Fact]
    public void From_SameGuid_ReturnsEqualIds()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(UserId.From(guid), UserId.From(guid));
    }
}