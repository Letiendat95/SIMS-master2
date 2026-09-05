namespace SIMS.Web.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; } // true = Present, false = Absent
    }
}