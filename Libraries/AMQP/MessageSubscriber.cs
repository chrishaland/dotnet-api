using Apache.NMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Libraries.AMQP;

public abstract class MessageSubscriber<TMessage> : BackgroundService where TMessage : class, new()
{
    private readonly string _messageQueueName;

    protected readonly ILogger _logger;
    protected readonly IServiceProvider _serviceProvider;

    public MessageSubscriber(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _messageQueueName = typeof(TMessage).Name;
    }

    protected abstract Task OnMessageReceived(TMessage message, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to listen on message queue '{MessageQueue}'", _messageQueueName);

        var connection = _serviceProvider.GetRequiredService<IConnection>();
        if (!connection.IsStarted) connection.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var session = connection.CreateSession();
            using var consumer = session.CreateConsumer(destination: session.GetQueue(_messageQueueName));

            var response = consumer.Receive(TimeSpan.FromSeconds(10));
            if (response == null) continue;

            if (response is not ITextMessage textMessage)
            {
                _logger.LogWarning("Message '{MessageId}' was received on queue, but the body was not in the correct format.", response.NMSMessageId);
                response.Acknowledge();
                continue;
            }

            try
            {
                _logger.LogInformation("Processing message '{MessageId}'", response.NMSMessageId);

                var message = JsonConvert.DeserializeObject<TMessage>(textMessage.Text) ?? new TMessage();

                await OnMessageReceived(message, stoppingToken);
                response.Acknowledge();

                _logger.LogInformation("Finished processing message '{MessageId}'", response.NMSMessageId);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogWarning(ex, "Unable to deserialize {MessageType} from {MessageId}", _messageQueueName, response.NMSMessageId);
                response.Acknowledge();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the received message '{MessageId}' with body:\n'{@Message}'", response.NMSMessageId, textMessage.Text);
            }
        }

        _logger.LogInformation("Cancellation token received. Stopping listening on message queue '{MessageQueue}'", _messageQueueName);
    }
}
