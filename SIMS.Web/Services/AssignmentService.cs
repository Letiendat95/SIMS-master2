using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services
{
    public class AssignmentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService _notificationService;

        public AssignmentService(AppDbContext context, IWebHostEnvironment env, NotificationService notificationService)
        {
            _context = context;
            _env = env;
            _notificationService = notificationService;
        }

        // Faculty tạo assignment
        public async Task CreateAssignmentAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Notify enrolled students
            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == assignment.CourseId)
                .Select(e => e.StudentId)
                .ToListAsync();

            if (enrolledStudentIds.Any())
            {
                var course = await _context.Courses.FindAsync(assignment.CourseId);
                await _notificationService.CreateForMultipleAsync(
                    enrolledStudentIds,
                    "New Assignment",
                    $"A new assignment \"{assignment.Title}\" has been created for {course?.CourseName}.",
                    "Assignment",
                    $"/Assignment/MyAssignment?id={assignment.AssignmentId}");
            }
        }

        // Lấy tất cả assignment theo course
        public async Task<List<Assignment>> GetByCourseAsync(int courseId)
            => await _context.Assignments
                .Include(a => a.Submissions)
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

        /// <summary>
        /// Thư mục chứa bài nộp. Đặt NGOÀI wwwroot để UseStaticFiles không phục vụ trực tiếp —
        /// mọi lượt tải đều phải đi qua AssignmentController.Download để kiểm tra quyền.
        /// </summary>
        public string SubmissionsFolder => Path.Combine(_env.ContentRootPath, "App_Data", "submissions");

        /// <summary>Đường dẫn tuyệt đối của bài nộp; null nếu FilePath không hợp lệ.</summary>
        public string? ResolveSubmissionPath(string? storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName)) return null;

            // Chỉ nhận tên file trần, chặn path traversal (../) từ dữ liệu cũ.
            var safeName = Path.GetFileName(storedFileName);
            if (string.IsNullOrWhiteSpace(safeName)) return null;

            return Path.Combine(SubmissionsFolder, safeName);
        }

        // Student nộp bài
        public async Task<string?> SubmitAsync(int assignmentId, int studentId, IFormFile file)
        {
            // Kiểm tra đã nộp chưa
            var exists = await _context.Submissions
                .AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
            if (exists) return "You have already submitted your work for this assignment.";

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return "Assignment not found.";

            // Hết hạn thì không cho nộp (trước đây chỉ chặn phía trình duyệt)
            if (DateTime.Now > assignment.DueDate)
                return "The deadline for this assignment has passed.";

            Directory.CreateDirectory(SubmissionsFolder);

            // Tên lưu trữ là GUID, không đoán được và không chứa dữ liệu người dùng
            var ext = Path.GetExtension(file.FileName);
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(SubmissionsFolder, storedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileName = Path.GetFileName(file.FileName),
                FilePath = storedName,
                SubmittedAt = DateTime.Now
            };

            _context.Submissions.Add(submission);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Không để lại file mồ côi nếu ghi DB thất bại
                if (File.Exists(fullPath)) File.Delete(fullPath);
                throw;
            }

            return null;
        }

        // Faculty chấm điểm
        public async Task<string?> GradeSubmissionAsync(int submissionId, string grade, string? feedback)
        {
            // Cùng bộ điểm hợp lệ với EnrollmentService, không nhận chuỗi tùy ý
            var normalized = (grade ?? string.Empty).Trim().ToUpperInvariant();
            if (!EnrollmentService.ValidGrades.Contains(normalized))
                return "Grade must be one of A, B, C, D or F.";

            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) return "Submission not found.";

            submission.Grade = normalized;
            submission.Feedback = feedback;
            submission.IsGraded = true;
            await _context.SaveChangesAsync();

            // Notify student
            var assignment = await _context.Assignments.FindAsync(submission.AssignmentId);
            await _notificationService.CreateAsync(
                submission.StudentId,
                "Assignment Graded",
                $"Your submission for \"{assignment?.Title}\" has been graded: {normalized}.",
                "Grade");

            return null;
        }

        // Lấy danh sách submission của 1 assignment (để Faculty xem)
        public async Task<List<Submission>> GetSubmissionsAsync(int assignmentId)
            => await _context.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

        // Lấy 1 bài nộp theo Id (dùng cho Download)
        public async Task<Submission?> GetSubmissionAsync(int submissionId)
            => await _context.Submissions.FindAsync(submissionId);

        // Lấy bài nộp của student
        public async Task<Submission?> GetStudentSubmissionAsync(int assignmentId, int studentId)
            => await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        public async Task<Assignment?> GetAssignmentAsync(int id)
            => await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions).ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }
}