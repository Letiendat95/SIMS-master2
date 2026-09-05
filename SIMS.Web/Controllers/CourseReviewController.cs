using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Student")]
    public class CourseReviewController : Controller
    {
        private readonly CourseReviewService _reviewService;
        private readonly CourseService _courseService;
        private readonly AccessControlService _access;

        public CourseReviewController(CourseReviewService reviewService, CourseService courseService,
            AccessControlService access)
        {
            _reviewService = reviewService;
            _courseService = courseService;
            _access = access;
        }

        public async Task<IActionResult> MyReviews()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var reviews = await _reviewService.GetReviewsByStudentAsync(int.Parse(userId));
            return View(reviews);
        }

        public async Task<IActionResult> Create(int courseId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var studentId = int.Parse(userId);

            // Chỉ được đánh giá lớp mình đã đăng ký
            if (!await _access.StudentEnrolledInCourseAsync(studentId, courseId))
                return RedirectToAction("AccessDenied", "Account");

            // Check if already reviewed
            var existingReview = await _reviewService.GetReviewByStudentAndCourseAsync(studentId, courseId);
            if (existingReview != null)
                return RedirectToAction("Edit", new { reviewId = existingReview.ReviewId });

            var course = await _courseService.GetCourseAsync(courseId.ToString());
            if (course == null) return NotFound();

            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseCode = course.CourseCode;

            var review = new CourseReview
            {
                StudentId = studentId,
                CourseId = courseId
            };

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseReview review)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            review.StudentId = int.Parse(userId);

            // Chỉ được đánh giá lớp mình đã đăng ký
            if (!await _access.StudentEnrolledInCourseAsync(review.StudentId, review.CourseId))
                return RedirectToAction("AccessDenied", "Account");

            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                var course = await _courseService.GetCourseAsync(review.CourseId.ToString());
                ViewBag.CourseName = course?.CourseName;
                ViewBag.CourseCode = course?.CourseCode;
                return View(review);
            }

            await _reviewService.CreateOrUpdateReviewAsync(review);
            TempData["Success"] = "Review submitted successfully!";
            return RedirectToAction("Detail", "Course", new { id = review.CourseId });
        }

        public async Task<IActionResult> Edit(int reviewId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var review = await _reviewService.GetByIdAsync(reviewId.ToString());
            if (review == null) return NotFound();

            // Check if owns the review
            if (review.StudentId != int.Parse(userId))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.CourseName = review.Course?.CourseName;
            ViewBag.CourseCode = review.Course?.CourseCode;

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseReview review)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var studentId = int.Parse(userId);

            // Xác định bản ghi bằng ReviewId thay vì tin vào CourseId gửi lên,
            // nếu không việc "sửa" có thể tạo ra đánh giá mới cho lớp khác.
            var existing = await _reviewService.GetByIdAsync(review.ReviewId.ToString());
            if (existing == null) return NotFound();
            if (existing.StudentId != studentId)
                return RedirectToAction("AccessDenied", "Account");

            ModelState.Remove("Student");
            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                var course = await _courseService.GetCourseAsync(existing.CourseId.ToString());
                ViewBag.CourseName = course?.CourseName;
                ViewBag.CourseCode = course?.CourseCode;
                return View(existing);
            }

            var error = await _reviewService.UpdateReviewAsync(
                review.ReviewId, studentId, review.Rating, review.Comment);

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("MyReviews");
            }

            TempData["Success"] = "Review updated successfully!";
            return RedirectToAction("Detail", "Course", new { id = existing.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int reviewId, int courseId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var review = await _reviewService.GetByIdAsync(reviewId.ToString());
            if (review == null) return NotFound();

            // Check if owns the review
            if (review.StudentId != int.Parse(userId))
                return RedirectToAction("AccessDenied", "Account");

            await _reviewService.DeleteReviewAsync(reviewId.ToString());
            TempData["Success"] = "Review deleted successfully!";
            return RedirectToAction("Detail", "Course", new { id = courseId });
        }
    }
}
