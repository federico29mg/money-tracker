using Transactions.Domain.Common;

namespace Transactions.UnitTests.Domain.Common;

public class UserIdTests
{
    [Fact]
    public void From_EmptyGuid_ThrowsDomainException() => Assert.Throws<DomainException>(() => UserId.From(Guid.Empty));

    [Fact]
    public void From_SameGuid_ReturnsEqualIds()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(UserId.From(guid), UserId.From(guid));
    }
}