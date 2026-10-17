using System.ComponentModel.DataAnnotations;

namespace Users.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;

    [Required] public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)] public int AccessTokenLifetimeMinutes { get; set; } = 60;

    [Required] public string SigningKey { get; init; } = string.Empty;
}