using WebApp.Models;

namespace WebApp.Services
{
    public class CourseService
    {
        private readonly HttpClient _http;

        public CourseService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Course>>("api/Courses") ?? new();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Course>($"api/Courses/{id}");
        }
        public async Task<bool> CreateAsync(Course course)
        {
            var response = await _http.PostAsJsonAsync("api/Courses", course);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> EnrollAsync(Enrollment enrollment)
        {
            var response = await _http.PostAsJsonAsync("api/Enrollment", enrollment);
            return response.IsSuccessStatusCode;
        }
    }
}