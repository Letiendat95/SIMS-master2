namespace SIMS.Web.ViewModels
{
    public class DashboardViewModel
    {
        // Counts
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }

        // Attendance
        public int TotalAttendanceRecords { get; set; }
        public int PresentCount { get; set; }
        public double AttendanceRate => TotalAttendanceRecords > 0
            ? Math.Round((double)PresentCount / TotalAttendanceRecords * 100, 1) : 0;

        // Grade distribution
        public int GradeA { get; set; }
        public int GradeB { get; set; }
        public int GradeC { get; set; }
        public int GradeD { get; set; }
        public int GradeF { get; set; }
        public int TotalGraded { get; set; }

        // Recent activity
        public List<string> RecentEnrollments { get; set; } = new();
    }
}
