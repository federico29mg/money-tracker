using System.ComponentModel.DataAnnotations;

namespace Transactions.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    // The topic the Users service publishes to, and this service's subscription to it.
    [Required] public string UsersTopicName { get; init; } = string.Empty;

    [Required] public string SubscriptionName { get; init; } = string.Empty;
}