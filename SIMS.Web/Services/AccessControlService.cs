using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;

namespace SIMS.Web.Services
{
    /// <summary>
    /// Kiểm tra quyền ở mức BẢN GHI (record-level).
    /// AuthorizeAttribute chỉ kiểm tra vai trò (role), không biết bản ghi nào thuộc về ai,
    /// nên mọi action nhận id từ URL đều phải kiểm tra thêm bằng service này.
    ///
    /// Quy tắc nghiệp vụ:
    /// - Giảng viên chỉ được truy cập khóa học mình phụ trách (Course.FacultyId == UserId của mình)
    ///   và chỉ được xem sinh viên đang học các khóa đó.
    /// - Sinh viên chỉ được truy cập dữ liệu của chính mình và khóa học mình đã đăng ký.
    /// </summary>
    public class AccessControlService
    {
        private readonly AppDbContext _context;

        public AccessControlService(AppDbContext context)
        {
            _context = context;
        }

        // ==================== FACULTY ====================

        /// <summary>Giảng viên có phụ trách khóa học này không?</summary>
        public async Task<bool> FacultyOwnsCourseAsync(int facultyId, int courseId)
            => await _context.Courses
                .AnyAsync(c => c.CourseId == courseId && c.FacultyId == facultyId);

        /// <summary>Sinh viên có đang học ít nhất một khóa của giảng viên này không?</summary>
        public async Task<bool> FacultyTeachesStudentAsync(int facultyId, int studentId)
            => await (from e in _context.Enrollments
                      join c in _context.Courses on e.CourseId equals c.CourseId
                      where e.StudentId == studentId && c.FacultyId == facultyId
                      select e.EnrollmentId).AnyAsync();

        /// <summary>Danh sách Id sinh viên đang học các khóa của giảng viên này.</summary>
        public async Task<List<int>> StudentIdsTaughtByAsync(int facultyId)
            => await (from e in _context.Enrollments
                      join c in _context.Courses on e.CourseId equals c.CourseId
                      where c.FacultyId == facultyId
                      select e.StudentId).Distinct().ToListAsync();

        /// <summary>Danh sách Id khóa học do giảng viên này phụ trách.</summary>
        public async Task<List<int>> CourseIdsTaughtByAsync(int facultyId)
            => await _context.Courses
                .Where(c => c.FacultyId == facultyId)
                .Select(c => c.CourseId)
                .ToListAsync();

        /// <summary>Bản ghi đăng ký (enrollment) này có thuộc khóa của giảng viên không?</summary>
        public async Task<bool> FacultyOwnsEnrollmentAsync(int facultyId, int enrollmentId)
            => await (from e in _context.Enrollments
                      join c in _context.Courses on e.CourseId equals c.CourseId
                      where e.EnrollmentId == enrollmentId && c.FacultyId == facultyId
                      select e.EnrollmentId).AnyAsync();

        /// <summary>Bài tập này có thuộc khóa của giảng viên không?</summary>
        public async Task<bool> FacultyOwnsAssignmentAsync(int facultyId, int assignmentId)
            => await (from a in _context.Assignments
                      join c in _context.Courses on a.CourseId equals c.CourseId
                      where a.AssignmentId == assignmentId && c.FacultyId == facultyId
                      select a.AssignmentId).AnyAsync();

        /// <summary>Bài nộp này có thuộc khóa của giảng viên không?</summary>
        public async Task<bool> FacultyOwnsSubmissionAsync(int facultyId, int submissionId)
            => await (from s in _context.Submissions
                      join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
                      join c in _context.Courses on a.CourseId equals c.CourseId
                      where s.SubmissionId == submissionId && c.FacultyId == facultyId
                      select s.SubmissionId).AnyAsync();

        // ==================== STUDENT ====================

        /// <summary>Sinh viên có đăng ký khóa học này không?</summary>
        public async Task<bool> StudentEnrolledInCourseAsync(int studentId, int courseId)
            => await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        /// <summary>Sinh viên có đăng ký khóa học chứa bài tập này không?</summary>
        public async Task<bool> StudentEnrolledInAssignmentCourseAsync(int studentId, int assignmentId)
            => await (from a in _context.Assignments
                      join e in _context.Enrollments on a.CourseId equals e.CourseId
                      where a.AssignmentId == assignmentId && e.StudentId == studentId
                      select a.AssignmentId).AnyAsync();
    }
}
