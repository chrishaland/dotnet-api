using Apache.NMS;
using Apache.NMS.AMQP;
using Libraries.AMQP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Host
{
    public static class AmqpExtentions
    {
        public static IAmqpServiceCollection AddAmqpService(this IServiceCollection services, IConfiguration configuration)
        {
            var uri = configuration.GetValue<string?>("amqp:uri") ?? string.Empty;

            var amqpServices = new AmqpServiceCollection(
                services: services,
                isAmqpConfigured: !string.IsNullOrEmpty(uri)
            );

            if (string.IsNullOrEmpty(uri)) return amqpServices;

            var username = configuration.GetValue<string>("amqp:username") ?? string.Empty;
            var password = configuration.GetValue<string>("amqp:password") ?? string.Empty;

            services.AddTransient(typeof(MessagePublisher<>));
            services.AddSingleton<IConnectionFactory>(new ConnectionFactory(
                userName: username,
                password: password,
                brokerUri: uri
            ));
            services.AddSingleton(s => s.GetRequiredService<IConnectionFactory>().CreateConnection());

            return amqpServices;
        }

        public interface IAmqpServiceCollection
        {
            IAmqpServiceCollection AddAmqpSubscriber<TSubscriber, TMessage>() where TSubscriber : MessageSubscriber<TMessage> where TMessage : class, new();
        }

        public class AmqpServiceCollection : IAmqpServiceCollection
        {
            private readonly IServiceCollection _services;
            private readonly bool _isAmqpConfigured;

            public AmqpServiceCollection(IServiceCollection services, bool isAmqpConfigured)
            {
                _services = services;
                _isAmqpConfigured = isAmqpConfigured;
            }

            public IAmqpServiceCollection AddAmqpSubscriber<TSubscriber, TMessage>()
                where TSubscriber : MessageSubscriber<TMessage>
                where TMessage : class, new()
            {
                if (!_isAmqpConfigured) return this;
                _services.AddHostedService<TSubscriber>();
                return this;
            }
        }
    }
}
