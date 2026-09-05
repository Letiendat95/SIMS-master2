using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Repositories
{
    public class CourseReviewRepository : IRepository<CourseReview>
    {
        private readonly AppDbContext _context;

        public CourseReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseReview>> GetAllAsync()
            => await _context.CourseReviews
                .Include(cr => cr.Student)
                .Include(cr => cr.Course)
                .ToListAsync();

        public async Task<CourseReview?> GetByIdAsync(string id)
        {
            if (!int.TryParse(id, out int reviewId))
                return null;

            return await _context.CourseReviews
                .Include(cr => cr.Student)
                .Include(cr => cr.Course)
                .FirstOrDefaultAsync(cr => cr.ReviewId == reviewId);
        }

        public async Task AddAsync(CourseReview item)
        {
            _context.CourseReviews.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CourseReview item)
        {
            _context.CourseReviews.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                _context.CourseReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CourseReview>> GetReviewsByCourseAsync(int courseId)
            => await _context.CourseReviews
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId)
                .OrderByDescending(cr => cr.ReviewDate)
                .ToListAsync();

        public async Task<CourseReview?> GetReviewByStudentAndCourseAsync(int studentId, int courseId)
            => await _context.CourseReviews
                .Include(cr => cr.Student)
                .Include(cr => cr.Course)
                .FirstOrDefaultAsync(cr => cr.StudentId == studentId && cr.CourseId == courseId);

        public async Task<List<CourseReview>> GetReviewsByStudentAsync(int studentId)
            => await _context.CourseReviews
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == studentId)
                .OrderByDescending(cr => cr.ReviewDate)
                .ToListAsync();
    }
}
