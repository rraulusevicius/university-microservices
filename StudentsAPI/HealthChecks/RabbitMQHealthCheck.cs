using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace StudentsAPI.HealthChecks
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly string _uri;

        public RabbitMQHealthCheck(string uri)
        {
            _uri = uri;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory { Uri = new Uri(_uri) };
                using var connection = await factory.CreateConnectionAsync(cancellationToken);
                using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("RabbitMQ connection successful");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ connection failed", ex);
            }
        }
    }
}
