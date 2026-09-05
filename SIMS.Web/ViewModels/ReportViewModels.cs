namespace SIMS.Web.ViewModels
{
    public class GradeReportViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int GradedCount { get; set; }
        public double AverageGPA { get; set; }
        public int GradeACount { get; set; }
        public int GradeBCount { get; set; }
        public int GradeCCount { get; set; }
        public int GradeDCount { get; set; }
        public int GradeFCount { get; set; }
        public int PassCount => GradeACount + GradeBCount + GradeCCount + GradeDCount;
        public int FailCount => GradeFCount;
        public double PassRate => GradedCount > 0 ? Math.Round((double)PassCount / GradedCount * 100, 1) : 0;
        public List<EnrollmentRow> Enrollments { get; set; } = new();
    }

    public class EnrollmentRow
    {
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public int Credits { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public double AttendanceRate => TotalRecords > 0 ? Math.Round((double)PresentCount / TotalRecords * 100, 1) : 0;
        public List<StudentAttendanceRow> StudentAttendances { get; set; } = new();
    }

    public class StudentAttendanceRow
    {
        public string StudentName { get; set; } = string.Empty;
        public int Present { get; set; }
        public int Absent { get; set; }
        public double Rate => (Present + Absent) > 0 ? Math.Round((double)Present / (Present + Absent) * 100, 1) : 0;
    }

    public class ReportsDashboardViewModel
    {
        public List<GradeReportViewModel> GradeReports { get; set; } = new();
        public List<AttendanceReportViewModel> AttendanceReports { get; set; } = new();

        // Overall stats
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalGraded { get; set; }
        public int OverallPassCount { get; set; }
        public int OverallFailCount { get; set; }
        // NOTE_PASS_RATE: công thức Overall Pass Rate (%) = số đạt / tổng đã chấm * 100, làm tròn 1 số lẻ.
        public double OverallPassRate => TotalGraded > 0 ? Math.Round((double)OverallPassCount / TotalGraded * 100, 1) : 0;
        public double OverallAvgGPA { get; set; }
    }
}
