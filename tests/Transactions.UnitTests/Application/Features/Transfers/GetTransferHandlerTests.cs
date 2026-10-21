using Transactions.Application.Common;
using Transactions.Application.Features.Transfers;
using Transactions.Application.Features.Transfers.GetTransfer;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transfers;

public class GetTransferHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _cashUsd = Account.Open(Owner, "Cash USD", AccountType.Cash, Money.Zero(Currency.Usd));

    private readonly Account _checking =
        Account.Open(Owner, "Checking", AccountType.Checking, Money.Zero(Currency.Cop));

    private readonly GetTransferHandler _handler;
    private readonly InMemoryTransferRepository _repository = new();
    private readonly Transfer _transfer;

    public GetTransferHandlerTests()
    {
        _transfer = Transfer.Record(
            _checking, _cashUsd, Money.Of(500_000m, Currency.Cop), Money.Of(120m, Currency.Usd),
            new DateOnly(2026, 10, 20), "Dollars for the trip");
        _repository.Add(_transfer);
        _handler = new GetTransferHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_OwnTransfer_ReturnsMappedResponse()
    {
        var response = await _handler.HandleAsync(
            new GetTransferQuery(Owner.Value, _transfer.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(
            new TransferResponse(
                _transfer.Id.Value,
                _checking.Id.Value,
                _cashUsd.Id.Value,
                new DateOnly(2026, 10, 20),
                500_000m,
                "COP",
                120m,
                "USD",
                "Dollars for the trip"),
            response);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersTransfer_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetTransferQuery(Guid.NewGuid(), _transfer.Id.Value), TestContext.Current.CancellationToken));
}