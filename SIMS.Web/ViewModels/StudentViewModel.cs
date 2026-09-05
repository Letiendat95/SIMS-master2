namespace SIMS.Web.ViewModels
{
    public class StudentViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public double Gpa { get; set; }
    }
}
