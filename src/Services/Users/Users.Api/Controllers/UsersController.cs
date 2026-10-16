using Microsoft.AspNetCore.Mvc;

using Users.Api.Contracts;
using Users.Application.Features.ChangeDefaultCurrency;
using Users.Application.Features.GetUser;
using Users.Application.Features.RegisterUser;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        RegisterUserRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.DisplayName, request.DefaultCurrencyCode);
        
        var userId = await handler.HandleAsync(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id = userId }, new RegisterUserResponse(userId));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        [FromServices] GetUserHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetUserQuery(id), cancellationToken);
    }

    [HttpPut("{id:guid}/default-currency")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeDefaultCurrency(
        Guid id,
        ChangeDefaultCurrencyRequest request,
        [FromServices] ChangeDefaultCurrencyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ChangeDefaultCurrencyCommand(id, request.CurrencyCode), cancellationToken);

        return NoContent();
    }
}