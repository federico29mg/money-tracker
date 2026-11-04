using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Transactions.Application.Features.Provisioning.ProvisionUser;
using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Messaging;

// Receives the Users service's events. A message is completed only after it's been handled; if handling throws,
// Service Bus delivers it again, up to the subscription's MaxDeliveryCount, then moves it to the dead-letter queue.
internal sealed partial class UserEventsConsumer(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<UserEventsConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var processor = client.CreateProcessor(
            options.Value.UsersTopicName,
            options.Value.SubscriptionName,
            new ServiceBusProcessorOptions { AutoCompleteMessages = false, MaxConcurrentCalls = 1 });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // The host is stopping.
        }

        await processor.StopProcessingAsync(CancellationToken.None);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;

        if (message.Subject != nameof(UserRegistered))
        {
            // The subscription receives every Users event; the ones this service doesn't need are just acknowledged.
            await args.CompleteMessageAsync(message, args.CancellationToken);
            return;
        }

        var userRegistered = message.Body.ToObjectFromJson<UserRegistered>(JsonSerializerOptions.Web);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ProvisionUserHandler>();

            await handler.HandleAsync(
                new ProvisionUserCommand(userRegistered!.UserId, userRegistered.DefaultCurrencyCode),
                args.CancellationToken);
        }
        catch (DomainException exception)
        {
            // Invalid data won't get better on a retry: park the message where someone can inspect it.
            LogRejected(exception, message.MessageId);
            await args.DeadLetterMessageAsync(
                message, "InvalidEvent", exception.Message, args.CancellationToken);
            return;
        }

        await args.CompleteMessageAsync(message, args.CancellationToken);
        LogProvisioned(userRegistered.UserId);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        LogProcessingFailed(args.Exception, args.ErrorSource, args.EntityPath);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Provisioned user {UserId}.")]
    private partial void LogProvisioned(Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Message {MessageId} was rejected and dead-lettered.")]
    private partial void LogRejected(Exception exception, string messageId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Processing Service Bus messages failed ({ErrorSource}, {EntityPath}).")]
    private partial void LogProcessingFailed(Exception exception, ServiceBusErrorSource errorSource, string entityPath);
}