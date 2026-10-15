using Users.Domain;
using Users.Domain.Common;

namespace Users.UnitTests;

public class UserTests
{
    private static User CreateUser(string displayName = "Ana") =>
        User.Register(UserId.New(), displayName, UserSettings.Default);

    [Fact]
    public void Register_ValidData_SetsProperties()
    {
        var id = UserId.New();

        var user = User.Register(id, "Ana", UserSettings.Default);

        Assert.Equal(id, user.Id);
        Assert.Equal("Ana", user.DisplayName);
        Assert.Equal(UserSettings.Default, user.Settings);
    }

    [Fact]
    public void Register_DisplayNameWithSpaces_TrimsDisplayName()
    {
        var user = CreateUser("  Ana  ");

        Assert.Equal("Ana", user.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_MissingDisplayName_ThrowsDomainException(string displayName) =>
        Assert.Throws<DomainException>(() => CreateUser(displayName));

    [Fact]
    public void Register_TooLongDisplayName_ThrowsDomainException()
    {
        var tooLong = new string('a', User.MaxDisplayNameLength + 1);

        Assert.Throws<DomainException>(() => CreateUser(tooLong));
    }

    [Fact]
    public void Register_MaxLengthDisplayName_CreatesUser()
    {
        var maxLengthName = new string('a', User.MaxDisplayNameLength);

        var user = CreateUser(maxLengthName);

        Assert.Equal(maxLengthName, user.DisplayName);
    }

    [Fact]
    public void Register_MaxLengthDisplayNameWithSpaces_TrimsAndCreatesUser()
    {
        var maxLengthName = new string('a', User.MaxDisplayNameLength);

        var user = CreateUser($"  {maxLengthName}  ");

        Assert.Equal(maxLengthName, user.DisplayName);
    }

    [Fact]
    public void Register_DefaultId_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => User.Register(default, "Ana", UserSettings.Default));

    [Fact]
    public void Rename_ValidName_ChangesDisplayName()
    {
        var user = CreateUser();

        user.Rename("Bea");

        Assert.Equal("Bea", user.DisplayName);
    }

    [Fact]
    public void Rename_InvalidName_KeepsPreviousDisplayName()
    {
        var user = CreateUser();

        Assert.Throws<DomainException>(() => user.Rename(" "));

        Assert.Equal("Ana", user.DisplayName);
    }

    [Fact]
    public void ChangeDefaultCurrency_NewCurrency_UpdatesSettings()
    {
        var user = CreateUser();

        user.ChangeDefaultCurrency(Currency.Usd);

        Assert.Equal(Currency.Usd, user.Settings.DefaultCurrency);
    }
}