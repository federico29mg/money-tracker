namespace Users.Domain;

public sealed record UserSettings
{
    private UserSettings(Currency defaultCurrency) => DefaultCurrency = defaultCurrency;
    public static UserSettings Default { get; } = new(Currency.Cop);

    public Currency DefaultCurrency { get; }

    public static UserSettings Create(Currency defaultCurrency)
    {
        ArgumentNullException.ThrowIfNull(defaultCurrency);
        return new UserSettings(defaultCurrency);
    }

    public UserSettings WithDefaultCurrency(Currency defaultCurrency) => Create(defaultCurrency);
}