using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StudentsAPI.Data;
using StudentsAPI.Protos;

namespace StudentsAPI.GrpcServices
{
    public class StudentGrpcService : StudentGrpc.StudentGrpcBase
    {
        private readonly StudentsAPIContext _context;
        private readonly ILogger<StudentGrpcService> _logger;

        public StudentGrpcService(StudentsAPIContext context, ILogger<StudentGrpcService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<StudentReply> GetStudent(StudentRequest request, ServerCallContext context)
        {
            var student = await _context.Student
                .Include(s => s.Enrollments)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == request.Id);

            if (student == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Student {request.Id} not found"));

            _logger.LogInformation("gRPC GetStudent called for ID={StudentId}", request.Id);
            return MapToReply(student);
        }

        public override async Task<StudentListReply> GetAllStudents(Empty request, ServerCallContext context)
        {
            var students = await _context.Student.AsNoTracking().ToListAsync();

            _logger.LogInformation("gRPC GetAllStudents called, returning {Count} students", students.Count);

            var reply = new StudentListReply();
            reply.Students.AddRange(students.Select(MapToReply));
            return reply;
        }

        private static StudentReply MapToReply(Models.Student student)
        {
            var reply = new StudentReply
            {
                Id = student.ID,
                LastName = student.LastName,
                FirstMidName = student.FirstMidName,
                EnrollmentDate = student.EnrollmentDate.ToString("yyyy-MM-dd")
            };

            if (student.Enrollments != null)
            {
                reply.Enrollments.AddRange(student.Enrollments.Select(e => new EnrollmentReply
                {
                    EnrollmentID = e.EnrollmentID,
                    CourseID = e.CourseID,
                    Title = e.Title,
                    Credits = e.Credits,
                    StudentID = e.StudentID,
                    Grade = e.Grade.HasValue ? (int)e.Grade : 0,
                    HasGrade = e.Grade.HasValue
                }));
            }

            return reply;
        }
    }
}