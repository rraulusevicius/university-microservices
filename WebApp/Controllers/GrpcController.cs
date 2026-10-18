using Microsoft.AspNetCore.Mvc;
using StudentsAPI.Protos;

namespace WebApp.Controllers
{
    public class GrpcController : Controller
    {
        private readonly StudentGrpc.StudentGrpcClient _grpcClient;

        public GrpcController(StudentGrpc.StudentGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        public async Task<IActionResult> Students()
        {
            var reply = await _grpcClient.GetAllStudentsAsync(new Empty());
            return View(reply.Students.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var reply = await _grpcClient.GetStudentAsync(new StudentRequest { Id = id });
            return View(reply);
        }
    }
}