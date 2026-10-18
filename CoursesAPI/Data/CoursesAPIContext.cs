using Microsoft.EntityFrameworkCore;
using CoursesAPI.Models;

namespace CoursesAPI.Data
{
    public class CoursesAPIContext : DbContext
    {
        public CoursesAPIContext(DbContextOptions<CoursesAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Course { get; set; }
    }
}
