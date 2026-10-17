namespace Users.Api.Contracts;

public sealed record RegisterUserRequest(string Email, string Password, string DisplayName, string? DefaultCurrencyCode)
{
    public override string ToString() =>
        $"{nameof(RegisterUserRequest)} {{ Email = {Email}, DisplayName = {DisplayName}, DefaultCurrencyCode = {DefaultCurrencyCode} }}";
}