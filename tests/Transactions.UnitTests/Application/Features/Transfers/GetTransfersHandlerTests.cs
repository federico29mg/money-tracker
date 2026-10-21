using Transactions.Application.Common;
using Transactions.Application.Features.Transfers;
using Transactions.Application.Features.Transfers.GetTransfers;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transfers;

public class GetTransfersHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));

    private readonly Account _checking =
        Account.Open(Owner, "Checking", AccountType.Checking, Money.Zero(Currency.Cop));

    private readonly GetTransfersHandler _handler;
    private readonly InMemoryTransferRepository _repository = new();
    private readonly Account _savings = Account.Open(Owner, "Savings", AccountType.Savings, Money.Zero(Currency.Cop));

    public GetTransfersHandlerTests()
    {
        Add(_checking, _card, 1, "Card payment");
        Add(_checking, _savings, 5, "Saving");
        Add(_savings, _checking, 10, "Back to checking");

        var someoneElse = UserId.From(Guid.NewGuid());
        Add(
            Account.Open(someoneElse, "A", AccountType.Cash, Money.Zero(Currency.Cop)),
            Account.Open(someoneElse, "B", AccountType.Cash, Money.Zero(Currency.Cop)),
            15,
            "Not mine");

        _handler = new GetTransfersHandler(_repository);
    }

    private void Add(Account from, Account to, int day, string description) =>
        _repository.Add(Transfer.Record(
            from, to, Money.Of(1_000m, Currency.Cop), null, new DateOnly(2026, 10, day), description));

    private Task<PagedResponse<TransferResponse>> GetAsync(GetTransfersQuery query) =>
        _handler.HandleAsync(query, TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_NoFilters_ReturnsOwnTransfersNewestFirst()
    {
        var page = await GetAsync(new GetTransfersQuery(Owner.Value));

        Assert.Equal(
            ["Back to checking", "Saving", "Card payment"],
            page.Items.Select(transfer => transfer.Description));
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_AccountFilter_MatchesBothDirections()
    {
        var page = await GetAsync(new GetTransfersQuery(Owner.Value, _savings.Id.Value));

        Assert.Equal(["Back to checking", "Saving"], page.Items.Select(transfer => transfer.Description));
    }

    [Fact]
    public async Task HandleAsync_DateRange_IncludesBothEnds()
    {
        var page = await GetAsync(new GetTransfersQuery(
            Owner.Value, From: new DateOnly(2026, 10, 1), To: new DateOnly(2026, 10, 5)));

        Assert.Equal(["Saving", "Card payment"], page.Items.Select(transfer => transfer.Description));
    }

    [Fact]
    public async Task HandleAsync_InvalidPaging_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            GetAsync(new GetTransfersQuery(Owner.Value, Page: 0)));
}