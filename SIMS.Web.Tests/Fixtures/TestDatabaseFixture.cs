using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Tests.Fixtures
{
    /// <summary>
    /// Shared InMemory database fixture for all tests.
    /// Uses TPH inheritance correctly: only add derived types (Student, Faculty, Administrator)
    /// to avoid tracking conflicts with base User type.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            SeedData(context);
            return context;
        }

        private void SeedData(AppDbContext context)
        {
            // Roles
            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Administrator" },
                new Role { RoleId = 2, RoleName = "Faculty", Description = "Faculty member" },
                new Role { RoleId = 3, RoleName = "Student", Description = "Student" }
            );

            // Administrator (User subtype) - UserId = 1
            context.Administrators.AddRange(
                new Administrator
                {
                    UserId = 1, Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@sims.edu", RoleId = 1,
                    AdminId = "A001", DateCreated = DateTime.Now.AddYears(-3)
                }
            );

            // Students (User subtypes) - UserId = 10, 11, 12
            context.Students.AddRange(
                new Student
                {
                    UserId = 10, Username = "john.doe",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    Email = "john.doe@student.sims.edu", RoleId = 3,
                    StudentId = "S001", FirstName = "John", LastName = "Doe",
                    AcademicProgram = "Computer Science",
                    EnrollmentDate = DateTime.Now.AddYears(-2)
                },
                new Student
                {
                    UserId = 11, Username = "jane.smith",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    Email = "jane.smith@student.sims.edu", RoleId = 3,
                    StudentId = "S002", FirstName = "Jane", LastName = "Smith",
                    AcademicProgram = "Computer Science",
                    EnrollmentDate = DateTime.Now.AddYears(-1)
                },
                new Student
                {
                    UserId = 12, Username = "michael.brown",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    Email = "michael.brown@student.sims.edu", RoleId = 3,
                    StudentId = "S003", FirstName = "Michael", LastName = "Brown",
                    AcademicProgram = "Mathematics",
                    EnrollmentDate = DateTime.Now.AddMonths(-6)
                }
            );

            // Faculties (User subtypes) - UserId = 2, 3
            context.Faculties.AddRange(
                new Faculty
                {
                    UserId = 2, Username = "dr.smith",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("faculty123"),
                    Email = "dr.smith@sims.edu", RoleId = 2,
                    FacultyId = "F001", FirstName = "Robert", LastName = "Smith",
                    Department = "Computer Science",
                    DateHired = DateTime.Now.AddYears(-5)
                },
                new Faculty
                {
                    UserId = 3, Username = "dr.johnson",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("faculty123"),
                    Email = "dr.johnson@sims.edu", RoleId = 2,
                    FacultyId = "F002", FirstName = "Emily", LastName = "Johnson",
                    Department = "Mathematics",
                    DateHired = DateTime.Now.AddYears(-3)
                }
            );

            context.SaveChanges();

            // Courses
            context.Courses.AddRange(
                new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Introduction to Programming", Credits = 3, Capacity = 30, FacultyId = 2, Status = "Active", StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now.AddMonths(6) },
                new Course { CourseId = 2, CourseCode = "CS201", CourseName = "Data Structures", Credits = 4, Capacity = 25, FacultyId = 2, Status = "Active", StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(9) },
                new Course { CourseId = 3, CourseCode = "MATH101", CourseName = "Calculus I", Credits = 3, Capacity = 35, FacultyId = 3, Status = "Active", StartDate = DateTime.Now.AddMonths(-4), EndDate = DateTime.Now.AddMonths(8) }
            );

            // Enrollments
            context.Enrollments.AddRange(
                new Enrollment { EnrollmentId = 1, StudentId = 10, CourseId = 1, Grade = "A", Status = "Active", EnrollmentDate = DateTime.Now.AddMonths(-5) },
                new Enrollment { EnrollmentId = 2, StudentId = 10, CourseId = 3, Grade = "B", Status = "Active", EnrollmentDate = DateTime.Now.AddMonths(-4) },
                new Enrollment { EnrollmentId = 3, StudentId = 11, CourseId = 1, Grade = "B", Status = "Active", EnrollmentDate = DateTime.Now.AddMonths(-4) },
                new Enrollment { EnrollmentId = 4, StudentId = 11, CourseId = 2, Grade = "", Status = "Active", EnrollmentDate = DateTime.Now.AddMonths(-2) },
                new Enrollment { EnrollmentId = 5, StudentId = 12, CourseId = 3, Grade = "C", Status = "Active", EnrollmentDate = DateTime.Now.AddMonths(-3) }
            );

            // Academic Records
            context.AcademicRecords.AddRange(
                new AcademicRecord { RecordId = 1, StudentId = 10, GPA = 3.5, TotalCreditsCompleted = 6, YearStarted = 2022 },
                new AcademicRecord { RecordId = 2, StudentId = 11, GPA = 3.0, TotalCreditsCompleted = 3, YearStarted = 2023 },
                new AcademicRecord { RecordId = 3, StudentId = 12, GPA = 2.0, TotalCreditsCompleted = 3, YearStarted = 2024 }
            );

            // Assignments
            context.Assignments.AddRange(
                new Assignment { AssignmentId = 1, Title = "HW1 - Hello World", Description = "Basic programming assignment", CourseId = 1, DueDate = DateTime.Now.AddDays(-10) },
                new Assignment { AssignmentId = 2, Title = "HW2 - Arrays", Description = "Array manipulation", CourseId = 1, DueDate = DateTime.Now.AddDays(5) },
                new Assignment { AssignmentId = 3, Title = "Midterm Exam", Description = "Midterm exam", CourseId = 2, DueDate = DateTime.Now.AddDays(15) }
            );

            // Submissions
            context.Submissions.AddRange(
                new Submission { SubmissionId = 1, AssignmentId = 1, StudentId = 10, FileName = "hello.cs", FilePath = "/uploads/hello.cs", Grade = "A", Feedback = "Excellent work", IsGraded = true, SubmittedAt = DateTime.Now.AddDays(-12) },
                new Submission { SubmissionId = 2, AssignmentId = 1, StudentId = 11, FileName = "hello_jane.cs", FilePath = "/uploads/hello_jane.cs", Grade = "B", Feedback = "Good", IsGraded = true, SubmittedAt = DateTime.Now.AddDays(-11) }
            );

            // Attendance
            context.Attendances.AddRange(
                new Attendance { AttendanceId = 1, CourseId = 1, StudentId = 10, Date = DateTime.Now.AddDays(-7), IsPresent = true },
                new Attendance { AttendanceId = 2, CourseId = 1, StudentId = 11, Date = DateTime.Now.AddDays(-7), IsPresent = false },
                new Attendance { AttendanceId = 3, CourseId = 1, StudentId = 10, Date = DateTime.Now.AddDays(-5), IsPresent = true },
                new Attendance { AttendanceId = 4, CourseId = 3, StudentId = 12, Date = DateTime.Now.AddDays(-3), IsPresent = true }
            );

            // Notifications
            context.Notifications.AddRange(
                new Notification { NotificationId = 1, UserId = 10, Title = "Welcome", Message = "Welcome to SIMS", Type = "General", IsRead = false, CreatedAt = DateTime.Now.AddDays(-5) },
                new Notification { NotificationId = 2, UserId = 10, Title = "Grade Updated", Message = "Your grade for CS101 has been updated to A", Type = "Grade", IsRead = true, CreatedAt = DateTime.Now.AddDays(-3) },
                new Notification { NotificationId = 3, UserId = 11, Title = "Enrolled", Message = "You have been enrolled in CS101", Type = "Enrollment", IsRead = false, CreatedAt = DateTime.Now.AddDays(-2) }
            );

            // Course Reviews
            context.CourseReviews.AddRange(
                new CourseReview { ReviewId = 1, StudentId = 10, CourseId = 1, Rating = 5, Comment = "Great course!", ReviewDate = DateTime.Now.AddDays(-10) },
                new CourseReview { ReviewId = 2, StudentId = 11, CourseId = 1, Rating = 4, Comment = "Very informative", ReviewDate = DateTime.Now.AddDays(-8) }
            );

            context.SaveChanges();
        }

        public void Dispose() { }
    }
}
