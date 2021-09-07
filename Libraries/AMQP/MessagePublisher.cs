using Apache.NMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Libraries.AMQP
{
    public sealed class MessagePublisher<TMessage> where TMessage : class
    {
        private readonly string _messageQueueName;

        private readonly ILogger<MessagePublisher<TMessage>> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MessagePublisher(ILogger<MessagePublisher<TMessage>> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _messageQueueName = typeof(TMessage).Name;
        }

        public void PublishMessage(TMessage message)
        {
            try
            {
                _logger.LogInformation("Publishing message type '{MessageQueue}'", _messageQueueName);

                var connection = _serviceProvider.GetRequiredService<IConnection>();

                using var session = connection.CreateSession();
                using var producer = session.CreateProducer(destination: session.GetQueue(_messageQueueName));

                var messageString = JsonConvert.SerializeObject(message);
                var request = session.CreateTextMessage(messageString);

                producer.Send(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message type '{MessageQueue}' to queue.", _messageQueueName);
            }
        }
    }
}
