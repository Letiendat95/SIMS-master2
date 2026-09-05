using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Student", "Admin", "Faculty")]
    public class StudentController : Controller
    {
        private readonly StudentService _studentService;
        private readonly EnrollmentService _enrollmentService;
        private readonly AccessControlService _access;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public StudentController(StudentService studentService, EnrollmentService enrollmentService,
            AccessControlService access, IWebHostEnvironment env)
        {
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _access = access;
            _env = env;
        }

        [Authorize("Admin", "Faculty")]
        public async Task<IActionResult> Index()
        {
            var students = await _studentService.GetAllStudentsAsync();

            // Giảng viên CHỈ được xem sinh viên đang học lớp mình phụ trách
            if (HttpContext.Session.GetString("Role") == "Faculty")
            {
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId))
                    return RedirectToAction("Login", "Account");

                var myStudentIds = await _access.StudentIdsTaughtByAsync(facultyId);
                students = students.Where(s => myStudentIds.Contains(s.UserId)).ToList();
            }

            return View(students);
        }

        [Authorize("Student", "Admin", "Faculty")]
        public async Task<IActionResult> Detail(string id)
        {
            if (!int.TryParse(id, out int targetId))
                return NotFound();

            if (!await CanViewStudentAsync(targetId))
                return RedirectToAction("AccessDenied", "Account");

            var student = await _studentService.GetStudentAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [Authorize("Student", "Admin", "Faculty")]
        public async Task<IActionResult> AcademicRecord(string id)
        {
            if (!int.TryParse(id, out int studentId))
                return NotFound();

            if (!await CanViewStudentAsync(studentId))
                return RedirectToAction("AccessDenied", "Account");

            var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(id);

            // Giảng viên chỉ thấy kết quả của CHÍNH các lớp mình dạy,
            // không thấy điểm của sinh viên ở lớp giảng viên khác.
            if (HttpContext.Session.GetString("Role") == "Faculty")
            {
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int facultyId))
                    return RedirectToAction("Login", "Account");

                var myCourseIds = await _access.CourseIdsTaughtByAsync(facultyId);
                enrollments = enrollments.Where(e => myCourseIds.Contains(e.CourseId)).ToList();
            }
            else
            {
                ViewBag.AcademicRecord = await _enrollmentService.GetAcademicRecordAsync(studentId);
            }

            return View(enrollments);
        }

        /// <summary>
        /// Admin xem được mọi sinh viên; sinh viên chỉ xem chính mình;
        /// giảng viên chỉ xem sinh viên đang học lớp mình phụ trách.
        /// </summary>
        private async Task<bool> CanViewStudentAsync(int studentId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin") return true;

            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                return false;

            if (role == "Student") return currentUserId == studentId;
            if (role == "Faculty") return await _access.FacultyTeachesStudentAsync(currentUserId, studentId);

            return false;
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var student = await _studentService.GetStudentAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, string email,
            string phoneNumber, string address, IFormFile? profilePicture)
        {
            var student = await _studentService.GetStudentAsync(id);
            if (student == null) return NotFound();

            student.Email = email;
            student.PhoneNumber = phoneNumber;
            student.Address = address;

            // Xử lý upload ảnh đại diện
            if (profilePicture != null && profilePicture.Length > 0)
            {
                // Validate file size
                if (profilePicture.Length > MaxFileSize)
                {
                    TempData["Error"] = "File size must not exceed 5MB.";
                    return RedirectToAction("Edit", new { id });
                }

                // Validate file type
                var ext = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Only JPG, JPEG, PNG, GIF, and WebP files are allowed.";
                    return RedirectToAction("Edit", new { id });
                }

                // Tạo thư mục nếu chưa có
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "profile-pictures");
                Directory.CreateDirectory(uploadDir);

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(student.ProfilePictureUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath,
                        student.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Lưu ảnh mới
                // Tên ngẫu nhiên để ảnh không bị đoán/liệt kê theo UserId + thời điểm
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                student.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
            }

            await _studentService.UpdateStudentAsync(student);

            // Cập nhật session nếu là chính mình
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id)
            {
                HttpContext.Session.SetString("ProfilePictureUrl", student.ProfilePictureUrl ?? "");
                HttpContext.Session.SetString("FullName", $"{student.FirstName} {student.LastName}");
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Detail", new { id });
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _studentService.DeleteStudentAsync(id);
            TempData["Success"] = "Student deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
