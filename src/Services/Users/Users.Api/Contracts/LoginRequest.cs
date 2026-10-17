namespace Users.Api.Contracts;

public sealed record LoginRequest(string Email, string Password)
{
    public override string ToString() => $"{nameof(LoginRequest)} {{ Email = {Email} }}";
}