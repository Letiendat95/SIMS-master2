using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Repositories
{
    public class StudentRepository : IRepository<Student>
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllAsync()
            => await _context.Students.Include(s => s.Role).ToListAsync();

        public async Task<Student?> GetByIdAsync(string id)
        {
            if (!int.TryParse(id, out int studentId))
                return null;

            return await _context.Students.Include(s => s.AcademicRecord)
                                       .Include(s => s.Enrollments)
                                       .FirstOrDefaultAsync(s => s.UserId == studentId);
        }

        public async Task AddAsync(Student item)
        {
            _context.Students.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student item)
        {
            _context.Students.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!int.TryParse(id, out int studentId)) return;

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .Include(s => s.AcademicRecord)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null) return;

            // Xóa attendance trước
            var attendances = _context.Attendances.Where(a => a.StudentId == studentId);
            _context.Attendances.RemoveRange(attendances);

            // Xóa submissions
            var submissions = _context.Submissions.Where(s => s.StudentId == studentId);
            _context.Submissions.RemoveRange(submissions);

            // Xóa course reviews
            var reviews = _context.CourseReviews.Where(r => r.StudentId == studentId);
            _context.CourseReviews.RemoveRange(reviews);

            // Xóa enrollments
            if (student.Enrollments.Any())
                _context.Enrollments.RemoveRange(student.Enrollments);

            // Xóa academic record
            if (student.AcademicRecord != null)
                _context.AcademicRecords.Remove(student.AcademicRecord);

            // Xóa student
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }





    }
}
