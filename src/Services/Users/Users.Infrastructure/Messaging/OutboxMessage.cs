namespace Users.Infrastructure.Messaging;

// An integration event waiting to be sent, stored in the same database transaction as the change that raised it.
internal sealed class OutboxMessage
{
    public const int MaxTypeLength = 200;
    public const int MaxErrorLength = 2000;

    public OutboxMessage(Guid id, string type, string content, DateTimeOffset occurredAt)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }

    // The event's name, e.g. "UserRegistered"; consumers use it to tell events apart.
    public string Type { get; }

    // The event serialized as JSON.
    public string Content { get; }
    public DateTimeOffset OccurredAt { get; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        Attempts++;
        LastError = null;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error.Length > MaxErrorLength ? error[..MaxErrorLength] : error;
    }
}