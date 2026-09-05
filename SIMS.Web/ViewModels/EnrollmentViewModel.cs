namespace SIMS.Web.ViewModels
{
    public class EnrollmentViewModel
    {
        public string EnrollmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
    }
}
