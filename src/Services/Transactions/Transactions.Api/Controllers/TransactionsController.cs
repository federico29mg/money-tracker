using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Transactions.Api.Authentication;
using Transactions.Api.Contracts;
using Transactions.Application.Common;
using Transactions.Application.Features.Transactions;
using Transactions.Application.Features.Transactions.GetTransaction;
using Transactions.Application.Features.Transactions.GetTransactions;
using Transactions.Application.Features.Transactions.RecordTransaction;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class TransactionsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<TransactionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetTransactions(
        [FromServices] GetTransactionsHandler handler,
        Guid? accountId = null,
        Guid? categoryId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        int page = 1,
        int pageSize = GetTransactionsQuery.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        await handler.HandleAsync(
            new GetTransactionsQuery(User.GetUserId(), accountId, categoryId, from, to, page, pageSize),
            cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(
        Guid id,
        [FromServices] GetTransactionHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetTransactionQuery(User.GetUserId(), id), cancellationToken);

    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedResponse>> RecordTransaction(
        RecordTransactionRequest request,
        [FromServices] RecordTransactionHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new RecordTransactionCommand(
                User.GetUserId(),
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Amount,
                request.CurrencyCode,
                request.Date,
                request.Name,
                request.Description),
            cancellationToken);

        return CreatedAtAction(nameof(GetTransaction), new { id }, new CreatedResponse(id));
    }
}