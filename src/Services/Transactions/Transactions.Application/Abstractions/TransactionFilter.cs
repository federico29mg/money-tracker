using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;

namespace Transactions.Application.Abstractions;

// Null criteria don't filter. From and To are inclusive.
public sealed record TransactionFilter(
    AccountId? AccountId = null,
    CategoryId? CategoryId = null,
    DateOnly? From = null,
    DateOnly? To = null);