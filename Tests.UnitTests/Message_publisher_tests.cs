using Apache.NMS;
using Libraries.AMQP;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests.UnitTests
{
    [TestFixture]
    public class Message_publisher_tests
    {
        private MessagePublisher<TestMessage> _sut;

        private readonly Mock<IMessageProducer> _messageProducerMock = new();
        private readonly Mock<IConnection> _connectionMock = new();
        private readonly Mock<ISession> _sessionMock = new();

        [SetUp]
        public void Before()
        {
            _sessionMock.Setup(m => m.CreateProducer(It.IsAny<IDestination>())).Returns(_messageProducerMock.Object);
            _connectionMock.Setup(m => m.CreateSession()).Returns(_sessionMock.Object);

            var logger = TestLoggerFactory.GetLogger<MessagePublisher<TestMessage>>();
            var serviceProvider = TestServiceCollection.CreateServiceProvider(services => 
            {
                services.AddTransient(_ => _connectionMock.Object);
            });

            _sut = new (logger, serviceProvider);
        }

        [Test]
        public void Should_publish_message()
        {
            _sut.PublishMessage(new());
            _messageProducerMock.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Once);
        }

        private class TestMessage { }
    }
}
