using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;
using SIMS.Web.ViewModels;

namespace SIMS.Web.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var students = await _context.Students.CountAsync();
            var faculty = await _context.Faculties.CountAsync();
            var courses = await _context.Courses.CountAsync();
            var activeCourses = await _context.Courses.CountAsync(c => c.Status == "Active");
            var enrollments = await _context.Enrollments.CountAsync();

            // Attendance
            var totalAttendance = await _context.Attendances.CountAsync();
            var presentCount = await _context.Attendances.CountAsync(a => a.IsPresent);

            // Grade distribution
            var gradedEnrollments = await _context.Enrollments
                .Where(e => !string.IsNullOrEmpty(e.Grade))
                .ToListAsync();

            var gradeA = gradedEnrollments.Count(e => e.Grade == "A");
            var gradeB = gradedEnrollments.Count(e => e.Grade == "B");
            var gradeC = gradedEnrollments.Count(e => e.Grade == "C");
            var gradeD = gradedEnrollments.Count(e => e.Grade == "D");
            var gradeF = gradedEnrollments.Count(e => e.Grade == "F");

            return new DashboardViewModel
            {
                TotalStudents = students,
                TotalFaculty = faculty,
                TotalCourses = courses,
                ActiveCourses = activeCourses,
                TotalEnrollments = enrollments,
                TotalAttendanceRecords = totalAttendance,
                PresentCount = presentCount,
                GradeA = gradeA,
                GradeB = gradeB,
                GradeC = gradeC,
                GradeD = gradeD,
                GradeF = gradeF,
                TotalGraded = gradedEnrollments.Count
            };
        }
    }
}
