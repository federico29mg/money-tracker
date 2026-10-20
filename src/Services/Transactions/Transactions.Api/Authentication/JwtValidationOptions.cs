using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Authentication;

// How this service checks tokens issued by Users. The signing keys are read from the Users discovery document.
public sealed class JwtValidationOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; init; } = string.Empty;

    [Required] public string Audience { get; init; } = string.Empty;

    [Required] [Url] public string MetadataAddress { get; init; } = string.Empty;

    // Only turned off for local development, where Users.Api runs over plain HTTP.
    public bool RequireHttpsMetadata { get; init; } = true;
}