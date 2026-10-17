namespace Users.Application.Features.Login;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc)
{
    public string TokenType => "Bearer";
}