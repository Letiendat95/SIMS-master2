using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services
{
    public class EnrollmentService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public EnrollmentService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Điểm hợp lệ; mọi giá trị khác đều bị từ chối để không làm hỏng GPA.
        public static readonly string[] ValidGrades = { "A", "B", "C", "D", "F" };

        // Điểm được tính là ĐẠT (F là trượt nên không tính vào tín chỉ tích lũy).
        public static readonly string[] PassingGrades = { "A", "B", "C", "D" };

        // Trả về null nếu thành công, hoặc thông báo lỗi nếu không hợp lệ.
        public async Task<string?> AssignStudentToCourseAsync(string studentId, string courseId)
        {
            if (!int.TryParse(studentId, out int stId) || !int.TryParse(courseId, out int cId))
                return "Invalid student or course.";

            var student = await _context.Students.FindAsync(stId);
            if (student == null)
                return "Student not found.";

            var course = await _context.Courses.FindAsync(cId);
            if (course == null)
                return "Course not found.";

            // Chống đăng ký trùng: 1 học sinh không thể đăng ký cùng 1 khóa 2 lần
            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == stId && e.CourseId == cId);
            if (alreadyEnrolled)
                return "This student is already enrolled in this course.";

            // Lớp đã đóng hoặc đã kết thúc thì không ghi danh nữa
            if (course.Status != "Active")
                return "This course is not open for enrollment.";
            if (course.EndDate < DateTime.Now)
                return "This course has already ended.";

            // Kiểm tra sức chứa của lớp
            int activeCount = await _context.Enrollments
                .CountAsync(e => e.CourseId == cId && e.Status == "Active");
            if (course.Capacity > 0 && activeCount >= course.Capacity)
                return "This course has reached its capacity.";

            var enrollment = new Enrollment
            {
                StudentId = stId,
                CourseId = cId,
                EnrollmentDate = DateTime.Now,
                Grade = "",
                Status = "Active"
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Notify student
            await _notificationService.CreateAsync(
                stId,
                "Enrolled in Course",
                $"You have been enrolled in \"{course.CourseName}\" ({course.CourseCode}).",
                "Enrollment",
                "/Student/AcademicRecord/" + stId);

            return null;
        }

        // Trả về null nếu thành công, hoặc thông báo lỗi.
        public async Task<string?> InputGradeAsync(string enrollmentId, string grade)
        {
            if (!int.TryParse(enrollmentId, out int eId))
                return "Invalid enrollment.";

            var normalized = (grade ?? string.Empty).Trim().ToUpperInvariant();
            if (!ValidGrades.Contains(normalized))
                return "Grade must be one of A, B, C, D or F.";

            var enrollment = await _context.Enrollments.FindAsync(eId);
            if (enrollment == null)
                return "Enrollment not found.";

            enrollment.Grade = normalized;
            await _context.SaveChangesAsync();

            // Notify student
            var course = await _context.Courses.FindAsync(enrollment.CourseId);
            await _notificationService.CreateAsync(
                enrollment.StudentId,
                "Grade Updated",
                $"Your grade for \"{course?.CourseName}\" has been updated to {normalized}.",
                "Grade",
                "/Student/AcademicRecord/" + enrollment.StudentId);

            // Auto-recalculate GPA
            await RecalculateGPAAsync(enrollment.StudentId);

            return null;
        }

        private async Task RecalculateGPAAsync(int studentId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            double newGPA = AcademicRecord.CalculateGPA(enrollments);

            // Chỉ tín chỉ của môn ĐẠT mới được tính là hoàn thành; trượt (F) thì không.
            int totalCredits = enrollments
                .Where(e => PassingGrades.Contains(e.Grade) && e.Course != null)
                .Sum(e => e.Course?.Credits ?? 0);

            var record = await _context.AcademicRecords
                .FirstOrDefaultAsync(r => r.StudentId == studentId);

            if (record == null)
            {
                // Sinh viên tạo trước khi có bản vá học bạ -> tạo mới để không mất GPA
                var student = await _context.Students.FindAsync(studentId);
                record = new AcademicRecord
                {
                    StudentId = studentId,
                    YearStarted = student?.EnrollmentDate.Year ?? DateTime.Now.Year
                };
                _context.AcademicRecords.Add(record);
            }

            record.GPA = newGPA;
            record.TotalCreditsCompleted = totalCredits;
            await _context.SaveChangesAsync();
        }

        public async Task AssignFacultyToCourseAsync(int facultyId, int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                course.FacultyId = facultyId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentAsync(string studentId)
        {
            if (!int.TryParse(studentId, out int stId))
                return new List<Enrollment>();

            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == stId)
                .ToListAsync();
        }

        public async Task<AcademicRecord?> GetAcademicRecordAsync(int studentId)
            => await _context.AcademicRecords.FirstOrDefaultAsync(r => r.StudentId == studentId);
    }
}
