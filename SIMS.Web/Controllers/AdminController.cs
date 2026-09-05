using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;
using SIMS.Web.ViewModels;

namespace SIMS.Web.Controllers
{
    [Authorize("Admin")]
    public class AdminController : Controller
    {
        private readonly StudentService _studentService;
        private readonly AuthenticationService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private readonly FacultyService _facultyService;
        private readonly RoleService _roleService;
        private readonly DashboardService _dashboardService;

        public AdminController(
            StudentService studentService,
            AuthenticationService authService,
            CourseService courseService,
            EnrollmentService enrollmentService,
            FacultyService facultyService,
            RoleService roleService,
            DashboardService dashboardService)
        {
            _studentService = studentService;
            _authService = authService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _facultyService = facultyService;
            _roleService = roleService;
            _dashboardService = dashboardService;
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Index()
        {
            var students = await _studentService.GetAllStudentsAsync();
            var dashboard = await _dashboardService.GetDashboardAsync();
            ViewBag.Dashboard = dashboard;
            return View(students);
        }

        [Authorize("Admin")]
        public IActionResult RegisterStudent() => View();

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> RegisterStudent(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Trùng username sẽ khiến đăng nhập trả về nhầm tài khoản (không xác định)
            if (await _authService.UsernameExistsAsync(model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var studentRole = await _authService.GetRoleByNameAsync("Student");

            var student = new Student
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = _authService.HashPassword(model.Password),
                RoleId = studentRole?.RoleId ?? 3,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                AcademicProgram = model.AcademicProgram,
                EnrollmentDate = DateTime.Now,
                // Tạo sẵn học bạ để GPA/tín chỉ cập nhật được khi chấm điểm sau này
                AcademicRecord = new AcademicRecord
                {
                    GPA = 0,
                    TotalCreditsCompleted = 0,
                    YearStarted = DateTime.Now.Year
                }
            };

            await _studentService.RegisterStudentAsync(student);
            return RedirectToAction("Index");
        }

        // ==================== MANAGE ROLES ====================

        [Authorize("Admin")]
        public async Task<IActionResult> ManageRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        [Authorize("Admin")]
        public async Task<IActionResult> RoleUsers(int roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null) return NotFound();

            ViewBag.Role = role;
            ViewBag.AllRoles = await _roleService.GetAllRolesAsync();
            var users = await _roleService.GetUsersByRoleAsync(roleId);
            return View(users);
        }

        [Authorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(int userId, int newRoleId, int returnRoleId)
        {
            var roleError = await _roleService.ChangeUserRoleAsync(userId, newRoleId);
            if (roleError != null)
                TempData["Error"] = roleError;
            else
                TempData["Success"] = "User role updated successfully!";

            return RedirectToAction("RoleUsers", new { roleId = returnRoleId });
        }

        [Authorize("Admin")]
        public async Task<IActionResult> SearchStudent(string keyword)
        {
            var students = await _studentService.GetAllStudentsAsync();

            // ✅ Nếu keyword rỗng hoặc chỉ có khoảng trắng → hiển thị tất cả
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var dashboard = await _dashboardService.GetDashboardAsync();
                ViewBag.Dashboard = dashboard;
                return View("Index", students);
            }

            // ✅ Nếu có keyword → tìm kiếm
            var result = students.Where(s =>
                (s.FirstName != null && s.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (s.LastName != null && s.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (s.Username != null && s.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (s.Email != null && s.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (s.PhoneNumber != null && s.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();

            var dashboard2 = await _dashboardService.GetDashboardAsync();
            ViewBag.Dashboard = dashboard2;
            return View("Index", result);
        }

        [Authorize("Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var student = await _studentService.GetStudentAsync(id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [Authorize("Admin")]
        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(string id)
        {
            await _studentService.DeleteStudentAsync(id);
            return RedirectToAction("Index");
        }

        // ==================== ASSIGN STUDENT TO COURSE ====================

        [Authorize("Admin")]
        public async Task<IActionResult> AssignStudentToCourse()
        {
            var vm = new AssignStudentToCourseViewModel
            {
                Students = await _studentService.GetAllStudentsAsync(),
                Courses = await _courseService.GetAllCoursesAsync()
            };
            return View(vm);
        }

        [Authorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudentToCourse(AssignStudentToCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Students = await _studentService.GetAllStudentsAsync();
                model.Courses = await _courseService.GetAllCoursesAsync();
                return View(model);
            }

            // ✅ Xử lý nhiều học sinh
            if (model.StudentIds != null && model.StudentIds.Count > 0)
            {
                int successCount = 0;
                int skipped = 0;
                foreach (var studentId in model.StudentIds)
                {
                    // AssignStudentToCourseAsync trả về null nếu thành công,
                    // hoặc lý do (trùng, hết chỗ, lớp đã đóng...) nếu bị bỏ qua.
                    var err = await _enrollmentService.AssignStudentToCourseAsync(
                        studentId.ToString(), model.CourseId.ToString());
                    if (err == null) successCount++;
                    else skipped++;
                }

                TempData["Success"] = $"✅ Successfully assigned {successCount} student(s) to course!";
                if (skipped > 0)
                    TempData["Error"] = $"{skipped} student(s) were skipped (already enrolled, course full, or closed).";
            }
            else
            {
                TempData["Error"] = "Please select at least one student";
                model.Students = await _studentService.GetAllStudentsAsync();
                model.Courses = await _courseService.GetAllCoursesAsync();
                return View(model);
            }

            return RedirectToAction("AssignStudentToCourse");
        }

        // ==================== ASSIGN FACULTY TO COURSE ====================

        [Authorize("Admin")]
        public async Task<IActionResult> AssignFacultyToCourse()
        {
            var vm = new AssignFacultyToCourseViewModel
            {
                Courses = await _courseService.GetAllCoursesAsync(),
                Faculties = await _facultyService.GetAllFacultiesAsync()
            };
            return View(vm);
        }

        [Authorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFacultyToCourse(AssignFacultyToCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _courseService.GetAllCoursesAsync();
                model.Faculties = await _facultyService.GetAllFacultiesAsync();
                return View(model);
            }

            // Phải là giảng viên có thật; nếu không, khóa học sẽ mất giảng viên
            // và không giảng viên nào truy cập được nữa.
            var faculties = await _facultyService.GetAllFacultiesAsync();
            if (!faculties.Any(f => f.UserId == model.FacultyId))
            {
                TempData["Error"] = "Please select a valid instructor.";
                return RedirectToAction("AssignFacultyToCourse");
            }

            var course = await _courseService.GetCourseAsync(model.CourseId.ToString());
            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction("AssignFacultyToCourse");
            }

            course.FacultyId = model.FacultyId;
            await _courseService.UpdateCourseAsync(course);

            TempData["Success"] = "Faculty assigned to course successfully!";
            return RedirectToAction("AssignFacultyToCourse");
        }
    }
}
