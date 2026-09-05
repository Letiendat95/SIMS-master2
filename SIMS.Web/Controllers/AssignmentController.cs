using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AssignmentService _assignmentService;
        private readonly AccessControlService _access;

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB, khớp với thông báo trên giao diện

        public AssignmentController(AssignmentService assignmentService, AccessControlService access)
        {
            _assignmentService = assignmentService;
            _access = access;
        }

        /// <summary>Giảng viên đang đăng nhập có phụ trách khóa học này không?</summary>
        private async Task<bool> FacultyOwnsCourseAsync(int courseId)
            => int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId)
               && await _access.FacultyOwnsCourseAsync(facultyId, courseId);

        [Authorize("Faculty")]
        public async Task<IActionResult> Create(int courseId)
        {
            // Chỉ tạo bài tập cho lớp do chính mình phụ trách
            if (!await FacultyOwnsCourseAsync(courseId))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.CourseId = courseId;
            return View();
        }

        [Authorize("Faculty")]
        [HttpPost]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            if (!await FacultyOwnsCourseAsync(assignment.CourseId))
                return RedirectToAction("AccessDenied", "Account");

            // Bỏ qua validation lỗi do navigation property
            ModelState.Remove("Course");
            ModelState.Remove("Submissions");

            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = assignment.CourseId; // giữ lại CourseId khi lỗi
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View(assignment);
            }

            await _assignmentService.CreateAssignmentAsync(assignment);
            return RedirectToAction("Detail", "Course", new { id = assignment.CourseId });
        }

        [Authorize("Faculty")]
        public async Task<IActionResult> Detail(int id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId)
                || !await _access.FacultyOwnsAssignmentAsync(facultyId, id))
                return RedirectToAction("AccessDenied", "Account");

            var assignment = await _assignmentService.GetAssignmentAsync(id);
            if (assignment == null) return NotFound();
            return View(assignment);
        }

        [Authorize("Student")]
        [HttpPost]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile file)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int studentId))
                return RedirectToAction("Login", "Account");

            // Chỉ nộp bài cho lớp mình đã đăng ký
            if (!await _access.StudentEnrolledInAssignmentCourseAsync(studentId, assignmentId))
                return RedirectToAction("AccessDenied", "Account");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to submit.";
                return RedirectToAction("MyAssignment", new { id = assignmentId });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".zip" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                TempData["Error"] = "Only PDF, Word, or ZIP files are accepted.";
                return RedirectToAction("MyAssignment", new { id = assignmentId });
            }

            if (file.Length > MaxFileSize)
            {
                TempData["Error"] = "File size must not exceed 10MB.";
                return RedirectToAction("MyAssignment", new { id = assignmentId });
            }

            var error = await _assignmentService.SubmitAsync(assignmentId, studentId, file);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Submitted successfully!";

            return RedirectToAction("MyAssignment", new { id = assignmentId });
        }

        /// <summary>
        /// Tải bài nộp. File nằm ngoài wwwroot nên đây là lối vào DUY NHẤT,
        /// và chỉ chủ nhân bài nộp, giảng viên phụ trách lớp, hoặc Admin mới tải được.
        /// </summary>
        [Authorize("Student", "Faculty", "Admin")]
        [HttpGet]
        public async Task<IActionResult> Download(int submissionId)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                return RedirectToAction("Login", "Account");

            var submission = await _assignmentService.GetSubmissionAsync(submissionId);
            if (submission == null) return NotFound();

            var role = HttpContext.Session.GetString("Role");
            bool allowed = role == "Admin"
                || submission.StudentId == currentUserId
                || (role == "Faculty" && await _access.FacultyOwnsSubmissionAsync(currentUserId, submissionId));

            if (!allowed)
                return RedirectToAction("AccessDenied", "Account");

            var path = _assignmentService.ResolveSubmissionPath(submission.FilePath);
            if (path == null || !System.IO.File.Exists(path))
                return NotFound("The submitted file is no longer available on the server.");

            return PhysicalFile(path, "application/octet-stream", submission.FileName);
        }

        [Authorize("Student")]
        public async Task<IActionResult> MyAssignment(int id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int studentId))
                return RedirectToAction("Login", "Account");

            // Chỉ xem được bài tập của lớp mình đã đăng ký
            if (!await _access.StudentEnrolledInAssignmentCourseAsync(studentId, id))
                return RedirectToAction("AccessDenied", "Account");

            var assignment = await _assignmentService.GetAssignmentAsync(id);
            if (assignment == null) return NotFound();

            var submission = await _assignmentService.GetStudentSubmissionAsync(id, studentId);
            ViewBag.Submission = submission;
            return View(assignment);
        }

        [Authorize("Faculty")]
        [HttpPost]
        public async Task<IActionResult> Grade(int submissionId, string grade, string? feedback, int assignmentId)
        {
            // Chỉ chấm được bài nộp thuộc lớp của chính mình
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId)
                || !await _access.FacultyOwnsSubmissionAsync(facultyId, submissionId))
                return RedirectToAction("AccessDenied", "Account");

            var error = await _assignmentService.GradeSubmissionAsync(submissionId, grade, feedback);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Grade saved successfully!";

            return RedirectToAction("Detail", new { id = assignmentId });
        }
    }
}