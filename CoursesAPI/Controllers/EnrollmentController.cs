using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CoursesAPI.Models;
using CoursesAPI.Services;

namespace CoursesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IRabbitMQPublisher _publisher;

        public EnrollmentController(IRabbitMQPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Enrollment en)
        {
            await _publisher.PublishAsync(
                JsonConvert.SerializeObject(en),
                "enrollment_topic");

            return Ok(new { message = "Enrollment published to RabbitMQ" });
        }
    }
}
