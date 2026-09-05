using SIMS.Web.Models;
using SIMS.Web.Repositories;

namespace SIMS.Web.Services
{
    public class CourseService
    {
        private readonly IRepository<Course> _repository;

        public CourseService(IRepository<Course> repository)
        {
            _repository = repository;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
            => await _repository.GetAllAsync();

        public async Task<Course?> GetCourseAsync(string id)
        {
            if (!int.TryParse(id, out int courseId))
                return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task CreateCourseAsync(Course course)
        {
            // Auto-generated ID by database
            await _repository.AddAsync(course);
        }

        public async Task UpdateCourseAsync(Course course)
            => await _repository.UpdateAsync(course);

        public async Task DeleteCourseAsync(string id)
            => await _repository.DeleteAsync(id);
    }
}
