using RabbitMQ.Client;
using System.Text;

namespace CoursesAPI.Services
{
    public interface IRabbitMQPublisher : IAsyncDisposable
    {
        Task PublishAsync(string message, string routingKey);
    }

    public class RabbitMQPublisher : IRabbitMQPublisher, IAsyncDisposable
    {
        private readonly string _exchangeName;
        private readonly string _uri;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _initialized = false;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public RabbitMQPublisher(string uri, string exchangeName)
        {
            _uri = uri;
            _exchangeName = exchangeName;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_initialized) return;

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_uri)
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: false,
                    autoDelete: false);

                _initialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task PublishAsync(string message, string routingKey)
        {
            await EnsureInitializedAsync();

            var body = Encoding.UTF8.GetBytes(message);

            await _channel!.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: routingKey,
                body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
                await _channel.CloseAsync();
            if (_connection != null)
                await _connection.CloseAsync();
        }
    }
}
