using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Users.Api.Authentication;
using Users.Api.Contracts;
using Users.Application.Features.ChangeDefaultCurrency;
using Users.Application.Features.GetUser;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UsersController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(
        [FromServices] GetUserHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetUserQuery(User.GetUserId()), cancellationToken);

    [HttpPut("me/default-currency")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeDefaultCurrency(
        ChangeDefaultCurrencyRequest request,
        [FromServices] ChangeDefaultCurrencyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeDefaultCurrencyCommand(User.GetUserId(), request.CurrencyCode),
            cancellationToken);

        return NoContent();
    }
}