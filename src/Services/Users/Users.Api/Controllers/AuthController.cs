using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Users.Api.Contracts;
using Users.Application.Features.Login;
using Users.Application.Features.RegisterUser;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        RegisterUserRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.DisplayName,
            request.DefaultCurrencyCode);

        var userId = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(UsersController.GetCurrentUser),
            "Users",
            null,
            new RegisterUserResponse(userId));
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
}