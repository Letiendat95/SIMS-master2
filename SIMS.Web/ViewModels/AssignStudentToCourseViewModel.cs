using SIMS.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Web.ViewModels
{
    public class AssignStudentToCourseViewModel
    {
        // ✅ Danh sách StudentIds (cho phép chọn nhiều)
        [Required(ErrorMessage = "Please select at least one student")]
        public List<int> StudentIds { get; set; } = new();

        [Required(ErrorMessage = "Please choose a course.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a course.")]
        public int CourseId { get; set; }

        public List<Student>? Students { get; set; }
        public List<Course>? Courses { get; set; }
    }
}
