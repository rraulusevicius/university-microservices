using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StudentsAPI.Data;
using StudentsAPI.Models;
using System.Text;

namespace StudentsAPI
{
    public class EnrollmentDataCollector : IHostedService, IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _uri;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger<EnrollmentDataCollector> _logger;

        public EnrollmentDataCollector(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<EnrollmentDataCollector> logger)
        {
            _serviceProvider = serviceProvider;
            _uri = configuration["RabbitMQ:Uri"]
                ?? throw new InvalidOperationException("RabbitMQ:Uri is not configured (set RabbitMQ__Uri)");
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Retry loop - RabbitMQ may not be ready yet
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(_uri)
                    };

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    await _channel.ExchangeDeclareAsync(
                        exchange: "enrollment_exchange",
                        type: ExchangeType.Topic,
                        durable: false,
                        autoDelete: false,
                        cancellationToken: cancellationToken);

                    await _channel.QueueDeclareAsync(
                        queue: "enrollment_queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        cancellationToken: cancellationToken);

                    await _channel.QueueBindAsync(
                        queue: "enrollment_queue",
                        exchange: "enrollment_exchange",
                        routingKey: "enrollment_topic",
                        cancellationToken: cancellationToken);

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.ReceivedAsync += async (sender, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            var enrollment = JsonConvert.DeserializeObject<Enrollment>(message);

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<StudentsAPIContext>();
                                context.Enrollment.Add(enrollment!);
                                await context.SaveChangesAsync();
                            }

                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            _logger.LogInformation("Enrollment processed: StudentID={StudentId}, CourseID={CourseId}",
                                enrollment?.StudentID, enrollment?.CourseID);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing enrollment message");
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                        }
                    };

                    await _channel.BasicConsumeAsync(
                        queue: "enrollment_queue",
                        autoAck: false,
                        consumer: consumer,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("RabbitMQ subscriber started (RabbitMQ.Client 7.x)");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("RabbitMQ not ready, retrying in 5s... ({Attempt}/10): {Message}", i + 1, ex.Message);
                    await Task.Delay(5000, cancellationToken);
                }
            }

            _logger.LogError("Failed to connect to RabbitMQ after 10 attempts");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}
