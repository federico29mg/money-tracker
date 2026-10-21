using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Transactions.Api.Authentication;
using Transactions.Api.Contracts;
using Transactions.Application.Common;
using Transactions.Application.Features.Transfers;
using Transactions.Application.Features.Transfers.GetTransfer;
using Transactions.Application.Features.Transfers.GetTransfers;
using Transactions.Application.Features.Transfers.RecordTransfer;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class TransfersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<TransferResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<TransferResponse>>> GetTransfers(
        [FromServices] GetTransfersHandler handler,
        Guid? accountId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        int page = 1,
        int pageSize = GetTransfersQuery.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        await handler.HandleAsync(
            new GetTransfersQuery(User.GetUserId(), accountId, from, to, page, pageSize),
            cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferResponse>> GetTransfer(
        Guid id,
        [FromServices] GetTransferHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetTransferQuery(User.GetUserId(), id), cancellationToken);

    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedResponse>> RecordTransfer(
        RecordTransferRequest request,
        [FromServices] RecordTransferHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new RecordTransferCommand(
                User.GetUserId(),
                request.FromAccountId,
                request.ToAccountId,
                request.Amount,
                request.ReceivedAmount,
                request.Date,
                request.Description),
            cancellationToken);

        return CreatedAtAction(nameof(GetTransfer), new { id }, new CreatedResponse(id));
    }
}