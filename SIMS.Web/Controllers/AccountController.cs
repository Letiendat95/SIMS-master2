using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Services;
using SIMS.Web.ViewModels;

namespace SIMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authService;
        private readonly AppDbContext _context;

        public AccountController(AuthenticationService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Xóa mọi dữ liệu phiên cũ trước khi cấp danh tính mới
            HttpContext.Session.Clear();

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Role", user.Role?.RoleName ?? "");
            HttpContext.Session.SetString("FullName", $"{user.Username}");

            // Lưu avatar vào session cho tất cả roles
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl);
            }
            else
            {
                HttpContext.Session.SetString("ProfilePictureUrl", "");
            }

            // Lưu tên hiển thị theo role
            if (user.Role?.RoleName == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.UserId);
                if (student != null)
                {
                    HttpContext.Session.SetString("FullName", $"{student.FirstName} {student.LastName}");
                    if (!string.IsNullOrEmpty(student.ProfilePictureUrl))
                        HttpContext.Session.SetString("ProfilePictureUrl", student.ProfilePictureUrl);
                }
            }
            else if (user.Role?.RoleName == "Faculty")
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.UserId == user.UserId);
                if (faculty != null)
                {
                    HttpContext.Session.SetString("FullName", $"{faculty.FirstName} {faculty.LastName}");
                    if (!string.IsNullOrEmpty(faculty.ProfilePictureUrl))
                        HttpContext.Session.SetString("ProfilePictureUrl", faculty.ProfilePictureUrl);
                }
            }

            // Redirect theo role
            return user.Role?.RoleName switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Faculty" => RedirectToAction("Detail", "Faculty",
                                 new { id = user.UserId }),
                "Student" => RedirectToAction("AcademicRecord", "Student",
                                 new { id = user.UserId }),
                _ => RedirectToAction("Login")
            };
        }

        // POST để tránh bị đăng xuất ngoài ý muốn qua link/prefetch của trình duyệt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
