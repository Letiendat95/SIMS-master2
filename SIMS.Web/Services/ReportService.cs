using System.Text;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;
using SIMS.Web.Services.CsvImport;
using SIMS.Web.ViewModels;

namespace SIMS.Web.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportsDashboardViewModel> GetReportsDashboardAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Enrollments).ThenInclude(e => e.Student)
                .ToListAsync();

            // NOTE_TOTAL_STATS: "Total Students" và "Total Courses" ở đầu trang Report lấy từ đây.
            // Total Students = đếm số bản ghi trong bảng Students; Total Courses = đếm số khóa học.
            var totalStudents = await _context.Students.CountAsync();
            var totalCourses = courses.Count;
            var totalEnrollments = await _context.Enrollments.CountAsync();

            var gradeReports = new List<GradeReportViewModel>();
            var attendanceReports = new List<AttendanceReportViewModel>();

            int totalGraded = 0, totalPass = 0, totalFail = 0;

            // GPA toàn trường phải tính theo trọng số tín chỉ trên TOÀN BỘ bản ghi có điểm,
            // không lấy trung bình cộng của các trung bình từng lớp (lớp 2 SV nặng ngang lớp 50 SV).
            double totalWeightedPoints = 0;
            int totalCreditHours = 0;

            foreach (var course in courses)
            {
                var enrollments = course.Enrollments.ToList();
                var graded = enrollments.Where(e => !string.IsNullOrEmpty(e.Grade)).ToList();

                var gA = graded.Count(e => e.Grade == "A");
                var gB = graded.Count(e => e.Grade == "B");
                var gC = graded.Count(e => e.Grade == "C");
                var gD = graded.Count(e => e.Grade == "D");
                var gF = graded.Count(e => e.Grade == "F");
                var pass = gA + gB + gC + gD;

                // NOTE_PASS_RATE: Dữ liệu cho "Overall Pass Rate" và bảng "Pass/Fail Overview".
                // Cộng dồn qua từng khóa: totalGraded = tổng SV có điểm, totalPass = số đạt (A/B/C/D),
                // totalFail = số rớt (F). Pass Rate (%) = totalPass / totalGraded * 100 (tính ở ViewModel).
                totalGraded += graded.Count;
                totalPass += pass;
                totalFail += gF;

                // NOTE_AVG_GPA: Dữ liệu cho "Average GPA" toàn trường.
                // Điểm chữ -> điểm số (A=4..F=0) qua AcademicRecord.GradeToPoints, NHÂN số tín chỉ của khóa,
                // cộng dồn lại. GPA cuối = tổng(điểm*tín chỉ) / tổng tín chỉ (trung bình có trọng số tín chỉ).
                totalWeightedPoints += graded.Sum(e => AcademicRecord.GradeToPoints(e.Grade) * course.Credits);
                totalCreditHours += graded.Count * course.Credits;

                // NOTE_GRADE_REPORT_TABLE: Mỗi khóa tạo 1 dòng cho bảng "Grade Report by Course"
                // (sĩ số, số đã chấm, GPA lớp, phân bố A-F). Nguồn: Course + Enrollments (điểm).
                gradeReports.Add(new GradeReportViewModel
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    TotalEnrolled = enrollments.Count,
                    GradedCount = graded.Count,
                    AverageGPA = graded.Any() ? Math.Round(graded.Sum(e => AcademicRecord.GradeToPoints(e.Grade)) / graded.Count, 2) : 0,
                    GradeACount = gA,
                    GradeBCount = gB,
                    GradeCCount = gC,
                    GradeDCount = gD,
                    GradeFCount = gF,
                    Enrollments = enrollments.Select(e => new EnrollmentRow
                    {
                        StudentName = $"{e.Student?.FirstName} {e.Student?.LastName}",
                        Email = e.Student?.Email ?? "",
                        Grade = e.Grade,
                        Credits = course.Credits
                    }).ToList()
                });

                // NOTE_ATTENDANCE_REPORT_TABLE: Dữ liệu cho bảng "Attendance Report by Course".
                // Lấy toàn bộ bản ghi điểm danh của khóa từ bảng Attendances, rồi tổng hợp:
                // số buổi (ngày distinct), tổng lượt, số có mặt; Attendance Rate tính ở ViewModel.
                var attendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a => a.CourseId == course.CourseId)
                    .ToListAsync();

                var studentIds = attendance.Select(a => a.StudentId).Distinct().ToList();
                var studentAttendances = new List<StudentAttendanceRow>();

                foreach (var sid in studentIds)
                {
                    var records = attendance.Where(a => a.StudentId == sid).ToList();
                    studentAttendances.Add(new StudentAttendanceRow
                    {
                        StudentName = records.FirstOrDefault()?.Student?.FirstName + " " + records.FirstOrDefault()?.Student?.LastName,
                        Present = records.Count(a => a.IsPresent),
                        Absent = records.Count(a => !a.IsPresent)
                    });
                }

                attendanceReports.Add(new AttendanceReportViewModel
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    TotalSessions = attendance.Select(a => a.Date.Date).Distinct().Count(),
                    TotalRecords = attendance.Count,
                    PresentCount = attendance.Count(a => a.IsPresent),
                    StudentAttendances = studentAttendances
                });
            }

            return new ReportsDashboardViewModel
            {
                GradeReports = gradeReports,
                AttendanceReports = attendanceReports,
                TotalStudents = totalStudents,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments,
                TotalGraded = totalGraded,
                OverallPassCount = totalPass,
                OverallFailCount = totalFail,
                // NOTE_AVG_GPA: chốt số Average GPA = tổng(điểm*tín chỉ) / tổng tín chỉ.
                OverallAvgGPA = totalCreditHours > 0
                    ? Math.Round(totalWeightedPoints / totalCreditHours, 2)
                    : 0
            };
        }

        // Dựng nội dung CSV cho báo cáo điểm theo khóa (mỗi khóa một dòng),
        // đúng các cột đang hiển thị ở bảng "Grade Report by Course".
        public static string BuildGradeReportCsv(ReportsDashboardViewModel dashboard)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CsvWriter.BuildLine(
                "CourseCode", "CourseName", "TotalEnrolled", "GradedCount",
                "AverageGPA", "PassRate(%)", "A", "B", "C", "D", "F"));

            foreach (var r in dashboard.GradeReports.OrderBy(r => r.CourseCode))
            {
                sb.AppendLine(CsvWriter.BuildLine(
                    r.CourseCode, r.CourseName, r.TotalEnrolled, r.GradedCount,
                    r.AverageGPA.ToString("F2"), r.PassRate,
                    r.GradeACount, r.GradeBCount, r.GradeCCount, r.GradeDCount, r.GradeFCount));
            }

            return sb.ToString();
        }

        // Dựng nội dung CSV cho báo cáo điểm danh theo khóa (mỗi khóa một dòng),
        // đúng các cột đang hiển thị ở bảng "Attendance Report by Course".
        public static string BuildAttendanceReportCsv(ReportsDashboardViewModel dashboard)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CsvWriter.BuildLine(
                "CourseCode", "CourseName", "TotalSessions", "TotalRecords",
                "Present", "Absent", "AttendanceRate(%)"));

            // Chỉ xuất khóa đã có điểm danh, giống điều kiện hiển thị trên trang.
            foreach (var r in dashboard.AttendanceReports
                         .Where(a => a.TotalRecords > 0)
                         .OrderBy(a => a.CourseCode))
            {
                var absent = r.TotalRecords - r.PresentCount;
                sb.AppendLine(CsvWriter.BuildLine(
                    r.CourseCode, r.CourseName, r.TotalSessions, r.TotalRecords,
                    r.PresentCount, absent, r.AttendanceRate));
            }

            return sb.ToString();
        }
    }
}
