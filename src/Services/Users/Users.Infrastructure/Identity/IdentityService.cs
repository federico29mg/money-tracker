using Microsoft.AspNetCore.Identity;

using Users.Application.Abstractions;
using Users.Application.Common;
using Users.Domain.UserAggregate;

namespace Users.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task CreateUserAsync(UserId userId, string email, string password, CancellationToken cancellationToken)
    {
        var applicationUser = new ApplicationUser { Id = userId.Value, UserName = email, Email = email };

        var result = await userManager.CreateAsync(applicationUser, password);

        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(error => error.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        // Identity reports errors by code (PasswordTooShort, InvalidEmail, ...); group them per request field.
        var errors = result.Errors
            .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal) ? "Password" : "Email")
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        throw new ValidationException(errors);
    }

    public async Task<UserId?> ValidateCredentialsAsync(string email, string password,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null || await userManager.IsLockedOutAsync(applicationUser))
        {
            return null;
        }

        if (!await userManager.CheckPasswordAsync(applicationUser, password))
        {
            // CheckPasswordAsync doesn't count failures (SignInManager normally does); do it here to enable lockout.
            await userManager.AccessFailedAsync(applicationUser);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(applicationUser);

        return UserId.From(applicationUser.Id);
    }
}