using SIMS.Web.Models;
using SIMS.Web.Repositories;

namespace SIMS.Web.Services
{
    public class CourseReviewService
    {
        private readonly CourseReviewRepository _reviewRepository;

        public CourseReviewService(CourseReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<CourseReview>> GetReviewsByCourseAsync(int courseId)
            => await _reviewRepository.GetReviewsByCourseAsync(courseId);

        public async Task<CourseReview?> GetReviewByStudentAndCourseAsync(int studentId, int courseId)
            => await _reviewRepository.GetReviewByStudentAndCourseAsync(studentId, courseId);

        public async Task<List<CourseReview>> GetReviewsByStudentAsync(int studentId)
            => await _reviewRepository.GetReviewsByStudentAsync(studentId);

        public async Task<CourseReview?> GetByIdAsync(string id)
            => await _reviewRepository.GetByIdAsync(id);

        public async Task CreateOrUpdateReviewAsync(CourseReview review)
        {
            var existingReview = await _reviewRepository.GetReviewByStudentAndCourseAsync(
                review.StudentId, review.CourseId);

            if (existingReview != null)
            {
                existingReview.Rating = review.Rating;
                existingReview.Comment = review.Comment;
                existingReview.ReviewDate = DateTime.Now;
                await _reviewRepository.UpdateAsync(existingReview);
            }
            else
            {
                review.ReviewDate = DateTime.Now;
                await _reviewRepository.AddAsync(review);
            }
        }

        /// <summary>
        /// Cập nhật đúng bản đánh giá theo ReviewId và chỉ khi thuộc về sinh viên đó.
        /// KHÔNG cho đổi CourseId, tránh việc sửa đánh giá lại tạo ra đánh giá cho lớp khác.
        /// </summary>
        public async Task<string?> UpdateReviewAsync(int reviewId, int studentId, int rating, string comment)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId.ToString());
            if (review == null) return "Review not found.";
            if (review.StudentId != studentId) return "You can only edit your own review.";
            if (rating < 1 || rating > 5) return "Rating must be between 1 and 5.";

            review.Rating = rating;
            review.Comment = comment;
            review.ReviewDate = DateTime.Now;
            await _reviewRepository.UpdateAsync(review);
            return null;
        }

        public async Task DeleteReviewAsync(string id)
            => await _reviewRepository.DeleteAsync(id);

        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            var reviews = await _reviewRepository.GetReviewsByCourseAsync(courseId);
            if (reviews == null || !reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<int> GetReviewCountAsync(int courseId)
        {
            var reviews = await _reviewRepository.GetReviewsByCourseAsync(courseId);
            return reviews?.Count ?? 0;
        }
    }
}
