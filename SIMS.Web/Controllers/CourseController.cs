using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Student", "Admin", "Faculty")]
    public class CourseController : Controller
    {
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private readonly AccessControlService _access;
        private readonly AppDbContext _context;

        public CourseController(CourseService courseService, EnrollmentService enrollmentService,
            AccessControlService access, AppDbContext context)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _access = access;
            _context = context;
        }

        [Authorize("Student", "Admin", "Faculty")]
        public async Task<IActionResult> Index(string? search, string? status, string? department, int? credits)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var role = HttpContext.Session.GetString("Role");

            // Faculty chỉ thấy khóa học do CHÍNH MÌNH phụ trách
            if (role == "Faculty")
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int facultyId))
                {
                    courses = courses.Where(c => c.FacultyId == facultyId).ToList();
                }
            }

            // Student chỉ thấy khóa học mà mình ĐÃ được đăng ký (enrollment)
            if (role == "Student")
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int studentId))
                {
                    var enrolledCourseIds = await _context.Enrollments
                        .Where(e => e.StudentId == studentId)
                        .Select(e => e.CourseId)
                        .ToListAsync();

                    courses = courses.Where(c => enrolledCourseIds.Contains(c.CourseId)).ToList();
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c =>
                    c.CourseCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                courses = courses.Where(c => c.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(department))
            {
                courses = courses.Where(c =>
                    c.Faculty?.Department == department
                ).ToList();
            }

            if (credits.HasValue)
            {
                courses = courses.Where(c => c.Credits == credits.Value).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Department = department;
            ViewBag.Credits = credits;

            return View(courses);
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Create()
        {
            // ✅ Hiển thị danh sách giảng viên để Admin chọn
            var faculties = await _context.Faculties.ToListAsync();
            ViewBag.Faculties = faculties;
            return View();
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            ModelState.Remove("Faculty");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Assignments");

            // Giảng viên phải được chọn và phải tồn tại; nếu không, FacultyId = 0
            // sẽ gây lỗi khóa ngoại hoặc tạo khóa học không có giảng viên.
            var facultyValid = course.FacultyId > 0
                && await _context.Faculties.AnyAsync(f => f.UserId == course.FacultyId);
            if (!facultyValid)
                ModelState.AddModelError("FacultyId", "Please select a valid instructor.");

            if (!ModelState.IsValid)
            {
                var faculties = await _context.Faculties.ToListAsync();
                ViewBag.Faculties = faculties;
                return View(course);
            }

            // ✅ FacultyId được chọn từ form (Admin chọn)
            await _courseService.CreateCourseAsync(course);
            return RedirectToAction("Index");
        }

        [Authorize("Student", "Admin", "Faculty")]
        public async Task<IActionResult> Detail(string id)
        {
            if (!int.TryParse(id, out int courseId))
                return NotFound();

            // Trang này để lộ danh sách sinh viên của lớp cho Faculty/Admin,
            // nên phải chặn giảng viên xem lớp không phải của mình và
            // sinh viên xem lớp chưa đăng ký.
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                    return RedirectToAction("Login", "Account");

                bool allowed = role switch
                {
                    "Faculty" => await _access.FacultyOwnsCourseAsync(currentUserId, courseId),
                    "Student" => await _access.StudentEnrolledInCourseAsync(currentUserId, courseId),
                    _ => false
                };

                if (!allowed)
                    return RedirectToAction("AccessDenied", "Account");
            }

            var course = await _courseService.GetCourseAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [Authorize("Admin", "Faculty")]
        [HttpPost]
        public async Task<IActionResult> AssignStudent(string studentId, string courseId)
        {
            // Giảng viên chỉ được thêm sinh viên vào lớp của chính mình
            if (HttpContext.Session.GetString("Role") == "Faculty")
            {
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId)
                    || !int.TryParse(courseId, out int cId)
                    || !await _access.FacultyOwnsCourseAsync(facultyId, cId))
                    return RedirectToAction("AccessDenied", "Account");
            }

            var error = await _enrollmentService.AssignStudentToCourseAsync(studentId, courseId);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Student assigned to course successfully!";
            return RedirectToAction("Detail", new { id = courseId });
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _courseService.DeleteCourseAsync(id);
            return RedirectToAction("Index");
        }
    }
}
