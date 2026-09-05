using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Repositories
{
    public class CourseRepository : IRepository<Course>
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllAsync()
            => await _context.Courses.Include(c => c.Faculty).Include(c => c.Enrollments).ToListAsync();

        public async Task<Course?> GetByIdAsync(string id)
        {
            if (!int.TryParse(id, out int courseId))
                return null;

            return await _context.Courses
                .Include(c => c.Faculty)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task AddAsync(Course item)
        {
            _context.Courses.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course item)
        {
            _context.Courses.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!int.TryParse(id, out int courseId)) return;

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return;

            // Xóa attendance của course
            var attendances = _context.Attendances.Where(a => a.CourseId == courseId);
            _context.Attendances.RemoveRange(attendances);

            // Xóa course reviews
            var reviews = _context.CourseReviews.Where(r => r.CourseId == courseId);
            _context.CourseReviews.RemoveRange(reviews);

            // Xóa submissions của các assignment thuộc course
            if (course.Assignments.Any())
            {
                var assignmentIds = course.Assignments.Select(a => a.AssignmentId).ToList();
                var submissions = _context.Submissions.Where(s => assignmentIds.Contains(s.AssignmentId));
                _context.Submissions.RemoveRange(submissions);

                _context.Assignments.RemoveRange(course.Assignments);
            }

            // Xóa enrollments
            if (course.Enrollments.Any())
                _context.Enrollments.RemoveRange(course.Enrollments);

            // Xóa course
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}