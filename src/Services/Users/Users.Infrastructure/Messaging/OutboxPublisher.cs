using Azure.Messaging.ServiceBus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Messaging;

// Sends saved outbox messages to Service Bus, oldest first. A message can be sent twice (if saving that it was
// sent fails), so consumers must be idempotent; MessageId lets the broker and consumers recognise repeats.
internal sealed partial class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    ServiceBusSender sender,
    TimeProvider timeProvider,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval, timeProvider);

        do
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // E.g. the database is unavailable: log it and try again on the next tick instead of stopping the host.
                LogPublishingFailed(exception);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await sender.SendMessageAsync(
                    new ServiceBusMessage(message.Content)
                    {
                        MessageId = message.Id.ToString(),
                        Subject = message.Type,
                        ContentType = "application/json"
                    },
                    cancellationToken);

                message.MarkProcessed(timeProvider.GetUtcNow());
            }
            catch (ServiceBusException exception)
            {
                // The broker is unreachable or refused it: stop here so the next tick retries in the same order.
                message.RecordFailure(exception.Message);
                LogSendFailed(exception, message.Id, message.Type);
                break;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Publishing outbox messages failed.")]
    private partial void LogPublishingFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sending outbox message {MessageId} ({MessageType}) failed.")]
    private partial void LogSendFailed(Exception exception, Guid messageId, string messageType);
}