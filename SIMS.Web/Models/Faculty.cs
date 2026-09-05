namespace SIMS.Web.Models
{
    public class Faculty : User
    {
        public string FacultyId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime DateHired { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
