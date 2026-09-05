namespace SIMS.Web.ViewModels
{
    public class CourseViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int Capacity { get; set; }
        public int CurrentEnrollment { get; set; }
    }
}
