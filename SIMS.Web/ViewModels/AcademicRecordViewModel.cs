namespace SIMS.Web.ViewModels
{
    public class AcademicRecordViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public double Gpa { get; set; }
        public List<EnrollmentViewModel> Enrollments { get; set; } = new();
    }
}
