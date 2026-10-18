using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly CourseService _courseService;

        public CoursesController(CourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllAsync();
            return View(courses);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            await _courseService.CreateAsync(course);
            return RedirectToAction("Index");
        }
        public IActionResult Enroll()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(Enrollment enrollment)
        {
            await _courseService.EnrollAsync(enrollment);
            return RedirectToAction("Index");
        }
    }
}