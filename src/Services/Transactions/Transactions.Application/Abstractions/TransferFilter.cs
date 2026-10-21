using Transactions.Domain.AccountAggregate;

namespace Transactions.Application.Abstractions;

// Null criteria don't filter. AccountId matches transfers in either direction. From and To are inclusive.
public sealed record TransferFilter(AccountId? AccountId = null, DateOnly? From = null, DateOnly? To = null);