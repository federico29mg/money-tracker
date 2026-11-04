using System.ComponentModel.DataAnnotations;

namespace Users.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    // The topic Users publishes its integration events to.
    [Required] public string TopicName { get; init; } = string.Empty;
}