using Transactions.Domain.Common;

namespace Transactions.Api.Contracts;

public sealed record CreateCategoryRequest(string Name, TransactionType Type);