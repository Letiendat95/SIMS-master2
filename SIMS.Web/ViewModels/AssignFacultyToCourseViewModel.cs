using SIMS.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Web.ViewModels
{
    public class AssignFacultyToCourseViewModel
    {
        [Required(ErrorMessage = "Please choose a course.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a course.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please choose an instructor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose an instructor.")]
        public int FacultyId { get; set; }

        public List<Course>? Courses { get; set; }
        public List<Faculty>? Faculties { get; set; }
    }
}
