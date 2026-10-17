namespace Users.Application.Features.Login;

public sealed record LoginCommand(string Email, string Password)
{
    public override string ToString() => $"{nameof(LoginCommand)} {{ Email = {Email} }}";
}