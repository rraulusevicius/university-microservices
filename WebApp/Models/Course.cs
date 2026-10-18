namespace WebApp.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public required string Title { get; set; }
        public int Credits { get; set; }
    }
}