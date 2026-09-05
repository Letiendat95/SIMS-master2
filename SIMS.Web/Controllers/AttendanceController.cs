using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Data;
using SIMS.Web.Helpers;
using SIMS.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace SIMS.Web.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AttendanceService _attendanceService;
        private readonly AccessControlService _access;
        private readonly AppDbContext _context;

        public AttendanceController(AttendanceService attendanceService, AccessControlService access,
            AppDbContext context)
        {
            _attendanceService = attendanceService;
            _access = access;
            _context = context;
        }

        /// <summary>
        /// Trả về facultyId nếu giảng viên đang đăng nhập phụ trách khóa học này, ngược lại null.
        /// </summary>
        private async Task<int?> GetOwningFacultyIdAsync(int courseId)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId))
                return null;
            return await _access.FacultyOwnsCourseAsync(facultyId, courseId) ? facultyId : null;
        }

        // Bước 1: Faculty chọn Course
        [Authorize("Faculty")]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var courses = await _context.Courses
                .Include(c => c.Faculty)
                .ToListAsync();

            // Faculty chỉ điểm danh được khóa học do CHÍNH MÌNH phụ trách
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int facultyId))
            {
                courses = courses.Where(c => c.FacultyId == facultyId).ToList();
            }

            return View(courses);
        }

        // Bước 2: Faculty điểm danh theo ngày
        [Authorize("Faculty")]
        public async Task<IActionResult> TakeAttendance(int courseId, DateTime? date)
        {
            // Chỉ điểm danh được lớp do chính mình phụ trách
            if (await GetOwningFacultyIdAsync(courseId) == null)
                return RedirectToAction("AccessDenied", "Account");

            var selectedDate = date ?? DateTime.Today;
            var enrollments = await _attendanceService.GetEnrolledStudentsAsync(courseId);

            if (!enrollments.Any())
            {
                TempData["Error"] = "No students enrolled in this course.";
                return RedirectToAction("Index");
            }

            // Lấy điểm danh đã có của ngày đó (nếu có)
            var existing = await _attendanceService
                .GetAttendanceByDateAsync(courseId, selectedDate);

            ViewBag.CourseId = courseId;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ExistingAttendance = existing.ToDictionary(a => a.StudentId, a => a.IsPresent);

            var course = await _context.Courses.FindAsync(courseId);
            ViewBag.CourseName = $"{course?.CourseCode} — {course?.CourseName}";

            return View(enrollments);
        }

        // Bước 3: Lưu điểm danh
        [Authorize("Faculty")]
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(int courseId, DateTime date,
            Dictionary<int, bool> attendance)
        {
            if (await GetOwningFacultyIdAsync(courseId) == null)
                return RedirectToAction("AccessDenied", "Account");

            int rejected = await _attendanceService.SaveAttendanceAsync(courseId, date, attendance);
            TempData["Success"] = $"Attendance saved for {date:dd/MM/yyyy}.";
            if (rejected > 0)
                TempData["Error"] = $"{rejected} record(s) were ignored because those students are not enrolled in this course.";

            return RedirectToAction("History", new { courseId });
        }

        // Xem lịch sử điểm danh (Faculty)
        [Authorize("Faculty")]
        public async Task<IActionResult> History(int courseId)
        {
            if (await GetOwningFacultyIdAsync(courseId) == null)
                return RedirectToAction("AccessDenied", "Account");

            var records = await _attendanceService.GetAttendanceByCourseAsync(courseId);
            var summary = await _attendanceService.GetAttendanceSummaryAsync(courseId);
            var course = await _context.Courses.FindAsync(courseId);

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = $"{course?.CourseCode} — {course?.CourseName}";
            ViewBag.Summary = summary;

            return View(records);
        }

        // Student xem điểm danh của mình
        [Authorize("Student")]
        public async Task<IActionResult> MyAttendance(int courseId)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int studentId))
                return RedirectToAction("Login", "Account");

            // Chỉ xem được điểm danh của lớp mình đã đăng ký
            if (!await _access.StudentEnrolledInCourseAsync(studentId, courseId))
                return RedirectToAction("AccessDenied", "Account");

            var records = await _context.Attendances
                .Include(a => a.Course)
                .Where(a => a.StudentId == studentId && a.CourseId == courseId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var course = await _context.Courses.FindAsync(courseId);
            ViewBag.CourseName = $"{course?.CourseCode} — {course?.CourseName}";
            ViewBag.Present = records.Count(a => a.IsPresent);
            ViewBag.Absent = records.Count(a => !a.IsPresent);

            return View(records);
        }
    }
}