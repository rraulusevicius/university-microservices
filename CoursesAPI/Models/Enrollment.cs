namespace CoursesAPI.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }
        public int StudentID { get; set; }
    }
}
