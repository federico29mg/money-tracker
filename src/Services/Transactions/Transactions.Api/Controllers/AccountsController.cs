using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Transactions.Api.Authentication;
using Transactions.Api.Contracts;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Accounts.ArchiveAccount;
using Transactions.Application.Features.Accounts.GetAccount;
using Transactions.Application.Features.Accounts.GetAccounts;
using Transactions.Application.Features.Accounts.OpenAccount;
using Transactions.Application.Features.Accounts.RenameAccount;
using Transactions.Application.Features.Transactions.GetPendingTransactions;
using Transactions.Application.Features.Transactions.SettleTransactions;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AccountsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AccountResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> GetAccounts(
        [FromServices] GetAccountsHandler handler,
        bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        Ok(await handler.HandleAsync(new GetAccountsQuery(User.GetUserId(), includeArchived), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetAccount(
        Guid id,
        [FromServices] GetAccountHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetAccountQuery(User.GetUserId(), id), cancellationToken);

    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatedResponse>> OpenAccount(
        OpenAccountRequest request,
        [FromServices] OpenAccountHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new OpenAccountCommand(
                User.GetUserId(), request.Name, request.Type, request.CurrencyCode, request.OpeningBalance),
            cancellationToken);

        return CreatedAtAction(nameof(GetAccount), new { id }, new CreatedResponse(id));
    }

    [HttpPut("{id:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameAccount(
        Guid id,
        RenameRequest request,
        [FromServices] RenameAccountHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RenameAccountCommand(User.GetUserId(), id, request.Name), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveAccount(
        Guid id,
        [FromServices] ArchiveAccountHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ArchiveAccountCommand(User.GetUserId(), id), cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/pending-transactions")]
    [ProducesResponseType<PendingTransactionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PendingTransactionsResponse>> GetPendingTransactions(
        Guid id,
        [FromServices] GetPendingTransactionsHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetPendingTransactionsQuery(User.GetUserId(), id), cancellationToken);

    [HttpPost("{id:guid}/settlements")]
    [ProducesResponseType<SettlementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettlementResponse>> SettleTransactions(
        Guid id,
        SettleTransactionsRequest request,
        [FromServices] SettleTransactionsHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(
            new SettleTransactionsCommand(
                User.GetUserId(), id, request.TransactionIds, request.ExchangeRate, request.TotalPaid),
            cancellationToken);
}