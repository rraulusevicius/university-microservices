using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoursesAPI.Data;
using CoursesAPI.Models;
using MassTransit;
using Contracts;

namespace CoursesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly CoursesAPIContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CoursesController(CoursesAPIContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourse()
        {
            return await _context.Course.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return course;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.CourseID)
            {
                return BadRequest();
            }
            _context.Entry(course).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Course.Any(e => e.CourseID == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Course.Add(course);
            await _context.SaveChangesAsync();

            // Publish MassTransit event
            await _publishEndpoint.Publish(new CourseCreatedEvent
            {
                CourseID = course.CourseID,
                Title = course.Title,
                Credits = course.Credits,
                CreatedAt = DateTime.UtcNow
            });

            return CreatedAtAction("GetCourse", new { id = course.CourseID }, course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
                return NotFound();
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
