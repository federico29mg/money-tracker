using System.Security.Claims;

using Microsoft.IdentityModel.JsonWebTokens;

namespace Transactions.Api.Authentication;

internal static class ClaimsPrincipalExtensions
{
    // The JWT "sub" claim carries the user's id; [Authorize] guarantees a validated token is present.
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId)
            ? userId
            : throw new InvalidOperationException("The authenticated user has no valid 'sub' claim.");
}