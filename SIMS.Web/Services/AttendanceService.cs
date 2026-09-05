using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách student trong course để điểm danh
        public async Task<List<Enrollment>> GetEnrolledStudentsAsync(int courseId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .ToListAsync();

            // Phòng thủ: nếu dữ liệu lỡ có enrollment trùng cho cùng một sinh viên,
            // chỉ giữ MỘT dòng mỗi sinh viên. View điểm danh đặt tên radio theo StudentId,
            // hai dòng cùng StudentId sẽ gộp thành một nhóm radio và hiển thị/lưu sai.
            return enrollments
                .GroupBy(e => e.StudentId)
                .Select(g => g.First())
                .ToList();
        }

        // Lưu điểm danh. Trả về số bản ghi bị từ chối vì sinh viên không thuộc lớp.
        public async Task<int> SaveAttendanceAsync(int courseId, DateTime date,
            Dictionary<int, bool> attendanceData)
        {
            // Chỉ chấp nhận sinh viên ĐANG HỌC lớp này. Nếu không, một giảng viên có thể
            // POST UserId bất kỳ để ghi điểm danh (và lộ tên) sinh viên lớp khác.
            var enrolled = (await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .Select(e => e.StudentId)
                .ToListAsync()).ToHashSet();

            int rejected = 0;

            foreach (var entry in attendanceData)
            {
                var studentId = entry.Key;
                var isPresent = entry.Value;

                if (!enrolled.Contains(studentId))
                {
                    rejected++;
                    continue;
                }

                // Kiểm tra đã có record chưa
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a =>
                        a.CourseId == courseId &&
                        a.StudentId == studentId &&
                        a.Date.Date == date.Date);

                if (existing != null)
                {
                    // Cập nhật nếu đã có
                    existing.IsPresent = isPresent;
                }
                else
                {
                    // Tạo mới
                    _context.Attendances.Add(new Attendance
                    {
                        CourseId = courseId,
                        StudentId = studentId,
                        Date = date.Date,
                        IsPresent = isPresent
                    });
                }
            }

            await _context.SaveChangesAsync();
            return rejected;
        }

        // Lấy lịch sử điểm danh theo course
        public async Task<List<Attendance>> GetAttendanceByCourseAsync(int courseId)
            => await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

        // Lấy điểm danh của 1 ngày cụ thể
        public async Task<List<Attendance>> GetAttendanceByDateAsync(int courseId, DateTime date)
            => await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && a.Date.Date == date.Date)
                .ToListAsync();

        // Thống kê điểm danh của từng student trong course
        public async Task<Dictionary<int, (int Present, int Absent)>> GetAttendanceSummaryAsync(
            int courseId)
        {
            var records = await _context.Attendances
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            return records
                .GroupBy(a => a.StudentId)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Present: g.Count(a => a.IsPresent),
                        Absent: g.Count(a => !a.IsPresent)
                    )
                );
        }
    }
}