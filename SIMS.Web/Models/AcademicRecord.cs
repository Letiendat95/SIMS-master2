using System.ComponentModel.DataAnnotations;

namespace SIMS.Web.Models
{
    public class AcademicRecord
    {
        [Key]
        public int RecordId { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public double GPA { get; set; }
        public int TotalCreditsCompleted { get; set; }
        public int YearStarted { get; set; }

        public static double GradeToPoints(string grade)
        {
            return grade?.ToUpper() switch
            {
                "A" => 4.0,
                "B" => 3.0,
                "C" => 2.0,
                "D" => 1.0,
                "F" => 0.0,
                _ => 0.0
            };
        }

        public static double CalculateGPA(IEnumerable<Enrollment> gradedEnrollments)
        {
            var graded = gradedEnrollments
                .Where(e => !string.IsNullOrEmpty(e.Grade) && e.Course != null)
                .ToList();

            if (!graded.Any()) return 0.0;

            double totalPoints = graded.Sum(e => GradeToPoints(e.Grade) * (e.Course?.Credits ?? 0));
            int totalCredits = graded.Sum(e => e.Course?.Credits ?? 0);

            return totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 2) : 0.0;
        }
    }
}
