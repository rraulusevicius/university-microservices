using WebApp.Models;

namespace WebApp.Services
{
    public class StudentService
    {
        private readonly HttpClient _http;

        public StudentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Student>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Student>>("api/Students") ?? new();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Student>($"api/Students/{id}");
        }
        public async Task<bool> EnrollAsync(Enrollment enrollment)
        {
            var response = await _http.PostAsJsonAsync("api/Enrollment", enrollment);
            return response.IsSuccessStatusCode;
        }
    }
}