using Contracts;
using MassTransit;

namespace StudentsAPI.Consumers
{
    public class CourseCreatedConsumer : IConsumer<CourseCreatedEvent>
    {
        private readonly ILogger<CourseCreatedConsumer> _logger;

        public CourseCreatedConsumer(ILogger<CourseCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<CourseCreatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "MassTransit: Course created - ID={CourseID}, Title={Title}, Credits={Credits}, At={CreatedAt}",
                msg.CourseID, msg.Title, msg.Credits, msg.CreatedAt);

            return Task.CompletedTask;
        }
    }
}