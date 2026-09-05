using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Data;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace SIMS.Web.Controllers
{
    [Authorize("Faculty")]
    public class FacultyController : Controller
    {
        private readonly EnrollmentService _enrollmentService;
        private readonly AccessControlService _access;
        private readonly AppDbContext _context;

        public FacultyController(EnrollmentService enrollmentService, AccessControlService access,
            AppDbContext context)
        {
            _enrollmentService = enrollmentService;
            _access = access;
            _context = context;
        }

        // Xem Profile — chỉ được xem hồ sơ của CHÍNH MÌNH
        public async Task<IActionResult> Detail(string id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                return RedirectToAction("Login", "Account");

            if (!int.TryParse(id, out int targetId) || targetId != currentUserId)
                return RedirectToAction("AccessDenied", "Account");

            var faculty = await _context.Users
                .OfType<Faculty>()
                .Include(f => f.Courses)
                    .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(f => f.UserId == targetId);
            if (faculty == null) return NotFound();
            return View(faculty);
        }

        // Bước 1: Chọn Course
        public async Task<IActionResult> InputGrade()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var courses = await _context.Courses
                .Include(c => c.Faculty)
                .ToListAsync();

            // Faculty chỉ chấm điểm được khóa học do CHÍNH MÌNH phụ trách
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int facultyId))
            {
                courses = courses.Where(c => c.FacultyId == facultyId).ToList();
            }

            return View(courses);
        }

        // Bước 2: Chọn Student trong Course
        public async Task<IActionResult> SelectStudent(int courseId)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId))
                return RedirectToAction("Login", "Account");

            // Chỉ được xem sinh viên của lớp do CHÍNH MÌNH phụ trách
            if (!await _access.FacultyOwnsCourseAsync(facultyId, courseId))
                return RedirectToAction("AccessDenied", "Account");

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            if (!enrollments.Any())
            {
                TempData["Error"] = "No students enrolled in this course.";
                return RedirectToAction("InputGrade");
            }

            return View(enrollments);
        }

        // Bước 3: Submit grade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitGrade(string enrollmentId, string grade, int courseId)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId))
                return RedirectToAction("Login", "Account");

            // Chỉ được chấm điểm bản ghi thuộc lớp của chính mình
            if (!int.TryParse(enrollmentId, out int eId)
                || !await _access.FacultyOwnsEnrollmentAsync(facultyId, eId))
                return RedirectToAction("AccessDenied", "Account");

            var error = await _enrollmentService.InputGradeAsync(enrollmentId, grade);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Grade saved successfully!";

            return RedirectToAction("SelectStudent", new { courseId });
        }
    }
}