using Microsoft.EntityFrameworkCore;
using StudentsAPI.Models;

namespace StudentsAPI.Data
{
    public class StudentsAPIContext : DbContext
    {
        public StudentsAPIContext(DbContextOptions<StudentsAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Student { get; set; }
        public DbSet<Enrollment> Enrollment { get; set; }
    }
}
