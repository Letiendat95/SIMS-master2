using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Helpers;
using SIMS.Web.Models;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Admin")]
    public class FacultyManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuthenticationService _authService;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FacultyManagementController(AppDbContext context, AuthenticationService authService,
            IWebHostEnvironment env)
        {
            _context = context;
            _authService = authService;
            _env = env;
        }

        // Danh sách giáo viên
        public async Task<IActionResult> Index()
        {
            var faculties = await _context.Faculties
                .Include(f => f.Role)
                .Include(f => f.Courses)
                .ToListAsync();
            return View(faculties);
        }

        // Xem chi tiết giáo viên
        public async Task<IActionResult> Detail(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Role)
                .Include(f => f.Courses)
                    .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(f => f.UserId == id);

            if (faculty == null) return NotFound();
            return View(faculty);
        }

        // Thêm giáo viên - GET
        public IActionResult Create() => View();

        // Thêm giáo viên - POST
        [HttpPost]
        public async Task<IActionResult> Create(string firstName, string lastName,
            string username, string email, string password, string department)
        {
            // Kiểm tra username đã tồn tại chưa
            var exists = await _context.Users
                .AnyAsync(u => u.Username == username);
            if (exists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View();
            }

            var facultyRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Faculty");

            var faculty = new Faculty
            {
                Username = username,
                Email = email,
                PasswordHash = _authService.HashPassword(password),
                RoleId = facultyRole?.RoleId ?? 2,
                FirstName = firstName,
                LastName = lastName,
                Department = department,
                DateHired = DateTime.Now,
            };

            _context.Users.Add(faculty);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Faculty {firstName} {lastName} added successfully!";
            return RedirectToAction("Index");
        }

        // Sửa giáo viên - GET
        public async Task<IActionResult> Edit(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();
            return View(faculty);
        }

        // Sửa giáo viên - POST
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string firstName, string lastName,
            string email, string department, IFormFile? profilePicture)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            faculty.FirstName = firstName;
            faculty.LastName = lastName;
            faculty.Email = email;
            faculty.Department = department;

            // Xử lý upload ảnh đại diện
            if (profilePicture != null && profilePicture.Length > 0)
            {
                if (profilePicture.Length > MaxFileSize)
                {
                    TempData["Error"] = "File size must not exceed 5MB.";
                    return RedirectToAction("Edit", new { id });
                }

                var ext = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Only JPG, JPEG, PNG, GIF, and WebP files are allowed.";
                    return RedirectToAction("Edit", new { id });
                }

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "profile-pictures");
                Directory.CreateDirectory(uploadDir);

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(faculty.ProfilePictureUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath,
                        faculty.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // Tên ngẫu nhiên để ảnh không bị đoán/liệt kê theo UserId + thời điểm
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await profilePicture.CopyToAsync(stream);

                faculty.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
            }

            await _context.SaveChangesAsync();

            // Cập nhật session nếu là chính mình
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                HttpContext.Session.SetString("ProfilePictureUrl", faculty.ProfilePictureUrl ?? "");
                HttpContext.Session.SetString("FullName", $"{faculty.FirstName} {faculty.LastName}");
            }

            TempData["Success"] = "Faculty updated successfully!";
            return RedirectToAction("Detail", new { id });
        }

        // Xóa giáo viên
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Courses)
                .FirstOrDefaultAsync(f => f.UserId == id);

            if (faculty == null)
                return RedirectToAction("Index");

            // Không cho xóa giảng viên còn phụ trách lớp (tránh xóa lây khóa học + enrollment)
            if (faculty.Courses.Any())
            {
                TempData["Error"] = "Cannot delete this faculty because they are still assigned to courses. Reassign those courses first.";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(faculty);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Faculty removed successfully.";
            return RedirectToAction("Index");
        }
    }
}