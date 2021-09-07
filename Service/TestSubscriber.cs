using Libraries.AMQP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class TestMessage
    {
        public string Message { get; set; } = "";
    }

    public class TestSubscriber : MessageSubscriber<TestMessage>
    {
        public TestSubscriber(ILogger<TestSubscriber> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                var publisher = serviceProvider.GetRequiredService<MessagePublisher<TestMessage>>();
                publisher.PublishMessage(new TestMessage { Message = "Init message" });
            });
        }

        protected override async Task OnMessageReceived(TestMessage message, CancellationToken ct)
        {
            _logger.LogInformation("Received message: '{Message}'", message.Message);
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            var publisher = _serviceProvider.GetRequiredService<MessagePublisher<TestMessage>>();
            publisher.PublishMessage(new TestMessage { Message = "Message" });
        }
    }
}
