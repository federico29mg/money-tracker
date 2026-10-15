using Users.Domain;

namespace Users.UnitTests;

public class UserSettingsTests
{
    [Fact]
    public void Create_SameCurrency_ReturnsEqualSettings()
    {
        Assert.Equal(UserSettings.Create(Currency.Usd), UserSettings.Create(Currency.Usd));
    }
    
    [Fact]
    public void WithDefaultCurrency_NewCurrency_ReturnsNewSettingsAndKeepsOriginal()
    {
        var original = UserSettings.Create(Currency.Cop);

        var changed = original.WithDefaultCurrency(Currency.Usd);

        Assert.Equal(Currency.Usd, changed.DefaultCurrency);
        Assert.Equal(Currency.Cop, original.DefaultCurrency);
    }
}