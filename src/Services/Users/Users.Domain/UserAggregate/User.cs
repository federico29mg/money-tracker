using Users.Domain.Common;

namespace Users.Domain.UserAggregate;

public sealed class User
{
    public const int MaxDisplayNameLength = 100;

    private User(UserId id, string displayName, UserSettings settings)
    {
        Id = id;
        DisplayName = displayName;
        Settings = settings;
    }

    // Used by EF Core when loading: it can't pass complex types to constructors and sets Settings itself.
    private User(UserId id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
        Settings = null!;
    }

    public UserId Id { get; }
    public string DisplayName { get; private set; }
    public UserSettings Settings { get; private set; }

    public static User Register(UserId id, string displayName, UserSettings settings)
    {
        if (id == default)
        {
            throw new DomainException("User id is required.");
        }

        ArgumentNullException.ThrowIfNull(settings);

        return new User(id, NormalizeDisplayName(displayName), settings);
    }

    public void Rename(string displayName) => DisplayName = NormalizeDisplayName(displayName);

    public void ChangeDefaultCurrency(Currency defaultCurrency) =>
        Settings = Settings.WithDefaultCurrency(defaultCurrency);

    private static string NormalizeDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Display name is required.");
        }

        var trimmed = displayName.Trim();

        if (trimmed.Length > MaxDisplayNameLength)
        {
            throw new DomainException($"Display name cannot exceed {MaxDisplayNameLength} characters.");
        }

        return trimmed;
    }
}