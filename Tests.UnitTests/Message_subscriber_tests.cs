using Apache.NMS;
using Libraries.AMQP;

namespace Tests.UnitTests;

[TestFixture]
public class Message_subscriber_tests
{
    private readonly Mock<IMessageConsumer> _messageConsumerMock = new();
    private readonly Mock<IConnection> _connectionMock = new();
    private readonly Mock<ISession> _sessionMock = new();

    private ILogger<TestSubscriber> _logger;
    private IServiceProvider _serviceProvider;

    [SetUp]
    public void Before()
    {
        _sessionMock.Setup(m => m.CreateConsumer(It.IsAny<IDestination>())).Returns(_messageConsumerMock.Object);
        _connectionMock.Setup(m => m.CreateSession()).Returns(_sessionMock.Object);

        _logger = TestLoggerFactory.GetLogger<TestSubscriber>();
        _serviceProvider = TestServiceCollection.CreateServiceProvider(services =>
        {
            services.AddTransient(_ => _connectionMock.Object);
        });
    }

    [Test]
    public async Task Message_subscriber_should_receive_text_message()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<ITextMessage>();
        messageMock.SetupGet(m => m.Text).Returns("{ message: \"test\"}");

        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.True);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    [Test]
    public async Task Message_subscriber_should_acknowledge_invalid_text_message()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<ITextMessage>();
        messageMock.SetupGet(m => m.Text).Returns("{ invalid_message = \"test\"}");

        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.False);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    [Test]
    public async Task Message_subscriber_should_not_process_map_messages()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<IMapMessage>();
        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.False);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    [Test]
    public async Task Message_subscriber_should_not_process_stream_messages()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<IStreamMessage>();

        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.False);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    [Test]
    public async Task Message_subscriber_should_not_process_object_messages()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<IObjectMessage>();

        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.False);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    [Test]
    public async Task Message_subscriber_should_not_process_bytes_messages()
    {
        var cts = new CancellationTokenSource();
        var messageMock = new Mock<IBytesMessage>();

        _messageConsumerMock.Setup(m => m.Receive(It.IsAny<TimeSpan>()))
            .Returns(messageMock.Object)
            .Callback(() => cts.Cancel());

        var sut = new TestSubscriber(_logger, _serviceProvider);
        await sut.Execute(cts.Token);

        Assert.That(sut.MessageReceived, Is.False);
        messageMock.Verify(m => m.Acknowledge(), Times.Once);
    }

    private class TestMessage
    {
        public string Message { get; set; }
    }

    private class TestSubscriber : MessageSubscriber<TestMessage>
    {
        public bool MessageReceived { get; set; } = false;

        public TestSubscriber(ILogger<TestSubscriber> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        internal Task Execute(CancellationToken ct) => ExecuteAsync(ct);

        protected override Task OnMessageReceived(TestMessage message, CancellationToken ct)
        {
            MessageReceived = true;
            return Task.CompletedTask;
        }
    }
}
