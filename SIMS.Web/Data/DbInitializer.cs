using Microsoft.EntityFrameworkCore;
using SIMS.Web.Models;
using BCrypt.Net;

namespace SIMS.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            if (context.Roles.Any()) return;

            var rng = new Random(42);
            string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw);

            // ======================== ROLES ========================
            var adminRole = new Role { RoleName = "Admin", Description = "Administrator role" };
            var facultyRole = new Role { RoleName = "Faculty", Description = "Faculty/Instructor role" };
            var studentRole = new Role { RoleName = "Student", Description = "Student role" };
            context.Roles.AddRange(adminRole, facultyRole, studentRole);
            context.SaveChanges();

            // ======================== ADMIN ========================
            var admin = new Administrator
            {
                Username = "admin",
                PasswordHash = Hash("admin123"),
                Email = "admin@sims.edu",
                RoleId = adminRole.RoleId,
                DateCreated = DateTime.Now.AddYears(-2)
            };
            context.Users.Add(admin);

            // ======================== FACULTY (5) ========================
            var faculties = new Faculty[]
            {
                new Faculty { Username = "dr.smith",      PasswordHash = Hash("faculty123"), Email = "dr.smith@sims.edu",      RoleId = facultyRole.RoleId, FirstName = "Robert",   LastName = "Smith",    Department = "Computer Science", DateHired = DateTime.Now.AddYears(-5) },
                new Faculty { Username = "dr.johnson",    PasswordHash = Hash("faculty123"), Email = "dr.johnson@sims.edu",    RoleId = facultyRole.RoleId, FirstName = "Maria",    LastName = "Johnson",  Department = "Mathematics",      DateHired = DateTime.Now.AddYears(-3) },
                new Faculty { Username = "dr.williams",   PasswordHash = Hash("faculty123"), Email = "dr.williams@sims.edu",   RoleId = facultyRole.RoleId, FirstName = "David",    LastName = "Williams", Department = "Physics",          DateHired = DateTime.Now.AddYears(-4) },
                new Faculty { Username = "dr.brown",      PasswordHash = Hash("faculty123"), Email = "dr.brown@sims.edu",      RoleId = facultyRole.RoleId, FirstName = "Jennifer",  LastName = "Brown",    Department = "Computer Science", DateHired = DateTime.Now.AddYears(-2) },
                new Faculty { Username = "dr.davis",      PasswordHash = Hash("faculty123"), Email = "dr.davis@sims.edu",      RoleId = facultyRole.RoleId, FirstName = "Michael",  LastName = "Davis",    Department = "Business",         DateHired = DateTime.Now.AddYears(-1) },
            };
            context.Users.AddRange(faculties);
            context.SaveChanges();

            // ======================== STUDENTS (15) ========================
            var students = new Student[]
            {
                new Student { Username = "john.doe",        PasswordHash = Hash("student123"), Email = "john.doe@student.sims.edu",        RoleId = studentRole.RoleId, FirstName = "John",      LastName = "Doe",        DateOfBirth = new DateTime(2003, 5, 15),  PhoneNumber = "555-0001", Address = "123 Main St",     City = "Boston", State = "MA", ZipCode = "02101", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-8) },
                new Student { Username = "jane.smith",       PasswordHash = Hash("student123"), Email = "jane.smith@student.sims.edu",       RoleId = studentRole.RoleId, FirstName = "Jane",      LastName = "Smith",      DateOfBirth = new DateTime(2003, 8, 22),  PhoneNumber = "555-0002", Address = "456 Oak Ave",     City = "Boston", State = "MA", ZipCode = "02102", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-8) },
                new Student { Username = "michael.brown",    PasswordHash = Hash("student123"), Email = "michael.brown@student.sims.edu",    RoleId = studentRole.RoleId, FirstName = "Michael",   LastName = "Brown",      DateOfBirth = new DateTime(2004, 1, 10),  PhoneNumber = "555-0003", Address = "789 Pine Rd",     City = "Boston", State = "MA", ZipCode = "02103", AcademicProgram = "Mathematics",       EnrollmentDate = DateTime.Now.AddMonths(-7) },
                new Student { Username = "emily.davis",      PasswordHash = Hash("student123"), Email = "emily.davis@student.sims.edu",      RoleId = studentRole.RoleId, FirstName = "Emily",     LastName = "Davis",      DateOfBirth = new DateTime(2003, 3, 18),  PhoneNumber = "555-0004", Address = "321 Elm St",      City = "Boston", State = "MA", ZipCode = "02104", AcademicProgram = "Physics",           EnrollmentDate = DateTime.Now.AddMonths(-7) },
                new Student { Username = "daniel.wilson",    PasswordHash = Hash("student123"), Email = "daniel.wilson@student.sims.edu",    RoleId = studentRole.RoleId, FirstName = "Daniel",    LastName = "Wilson",     DateOfBirth = new DateTime(2002, 11, 5), PhoneNumber = "555-0005", Address = "654 Maple Dr",    City = "Boston", State = "MA", ZipCode = "02105", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-6) },
                new Student { Username = "sarah.anderson",   PasswordHash = Hash("student123"), Email = "sarah.anderson@student.sims.edu",   RoleId = studentRole.RoleId, FirstName = "Sarah",     LastName = "Anderson",   DateOfBirth = new DateTime(2003, 7, 30), PhoneNumber = "555-0006", Address = "987 Cedar Ln",    City = "Boston", State = "MA", ZipCode = "02106", AcademicProgram = "Business",          EnrollmentDate = DateTime.Now.AddMonths(-6) },
                new Student { Username = "chris.taylor",     PasswordHash = Hash("student123"), Email = "chris.taylor@student.sims.edu",     RoleId = studentRole.RoleId, FirstName = "Chris",     LastName = "Taylor",     DateOfBirth = new DateTime(2004, 2, 14), PhoneNumber = "555-0007", Address = "147 Birch Way",   City = "Boston", State = "MA", ZipCode = "02107", AcademicProgram = "Mathematics",       EnrollmentDate = DateTime.Now.AddMonths(-5) },
                new Student { Username = "ashley.thomas",    PasswordHash = Hash("student123"), Email = "ashley.thomas@student.sims.edu",    RoleId = studentRole.RoleId, FirstName = "Ashley",    LastName = "Thomas",     DateOfBirth = new DateTime(2003, 9, 8),  PhoneNumber = "555-0008", Address = "258 Walnut St",   City = "Boston", State = "MA", ZipCode = "02108", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-5) },
                new Student { Username = "james.jackson",    PasswordHash = Hash("student123"), Email = "james.jackson@student.sims.edu",    RoleId = studentRole.RoleId, FirstName = "James",     LastName = "Jackson",    DateOfBirth = new DateTime(2002, 12, 25),PhoneNumber = "555-0009", Address = "369 Spruce Ct",   City = "Boston", State = "MA", ZipCode = "02109", AcademicProgram = "Physics",           EnrollmentDate = DateTime.Now.AddMonths(-4) },
                new Student { Username = "rachel.white",     PasswordHash = Hash("student123"), Email = "rachel.white@student.sims.edu",     RoleId = studentRole.RoleId, FirstName = "Rachel",    LastName = "White",      DateOfBirth = new DateTime(2003, 6, 20), PhoneNumber = "555-0010", Address = "741 Willow Ave",  City = "Boston", State = "MA", ZipCode = "02110", AcademicProgram = "Business",          EnrollmentDate = DateTime.Now.AddMonths(-4) },
                new Student { Username = "kevin.harris",     PasswordHash = Hash("student123"), Email = "kevin.harris@student.sims.edu",     RoleId = studentRole.RoleId, FirstName = "Kevin",     LastName = "Harris",     DateOfBirth = new DateTime(2004, 4, 3),  PhoneNumber = "555-0011", Address = "852 Ash Blvd",    City = "Boston", State = "MA", ZipCode = "02111", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-3) },
                new Student { Username = "lisa.martin",      PasswordHash = Hash("student123"), Email = "lisa.martin@student.sims.edu",      RoleId = studentRole.RoleId, FirstName = "Lisa",      LastName = "Martin",     DateOfBirth = new DateTime(2003, 10, 12),PhoneNumber = "555-0012", Address = "963 Poplar Rd",   City = "Boston", State = "MA", ZipCode = "02112", AcademicProgram = "Mathematics",       EnrollmentDate = DateTime.Now.AddMonths(-3) },
                new Student { Username = "brian.garcia",     PasswordHash = Hash("student123"), Email = "brian.garcia@student.sims.edu",     RoleId = studentRole.RoleId, FirstName = "Brian",     LastName = "Garcia",     DateOfBirth = new DateTime(2003, 1, 28), PhoneNumber = "555-0013", Address = "159 Holly Ln",    City = "Boston", State = "MA", ZipCode = "02113", AcademicProgram = "Physics",           EnrollmentDate = DateTime.Now.AddMonths(-2) },
                new Student { Username = "nicole.clark",     PasswordHash = Hash("student123"), Email = "nicole.clark@student.sims.edu",     RoleId = studentRole.RoleId, FirstName = "Nicole",    LastName = "Clark",      DateOfBirth = new DateTime(2004, 6, 7),  PhoneNumber = "555-0014", Address = "357 Chestnut St", City = "Boston", State = "MA", ZipCode = "02114", AcademicProgram = "Business",          EnrollmentDate = DateTime.Now.AddMonths(-2) },
                new Student { Username = "alex.rodriguez",   PasswordHash = Hash("student123"), Email = "alex.rodriguez@student.sims.edu",   RoleId = studentRole.RoleId, FirstName = "Alex",      LastName = "Rodriguez",  DateOfBirth = new DateTime(2003, 4, 19), PhoneNumber = "555-0015", Address = "486 Sycamore Dr", City = "Boston", State = "MA", ZipCode = "02115", AcademicProgram = "Computer Science",  EnrollmentDate = DateTime.Now.AddMonths(-1) },
            };
            context.Users.AddRange(students);
            context.SaveChanges();

            // ======================== COURSES (8) ========================
            var courses = new Course[]
            {
                new Course { CourseCode = "CS101",    CourseName = "Introduction to Programming",  Description = "Learn the basics of programming with C#",                        Credits = 3, Capacity = 30, FacultyId = faculties[0].UserId, StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now.AddMonths(2), Status = "Active" },
                new Course { CourseCode = "CS201",    CourseName = "Advanced C# Programming",      Description = "Master advanced OOP, LINQ, and async patterns in C#",            Credits = 4, Capacity = 25, FacultyId = faculties[0].UserId, StartDate = DateTime.Now.AddMonths(-5), EndDate = DateTime.Now.AddMonths(3), Status = "Active" },
                new Course { CourseCode = "CS301",    CourseName = "Data Structures & Algorithms", Description = "Arrays, linked lists, trees, graphs, sorting and searching",     Credits = 4, Capacity = 30, FacultyId = faculties[3].UserId, StartDate = DateTime.Now.AddMonths(-4), EndDate = DateTime.Now.AddMonths(4), Status = "Active" },
                new Course { CourseCode = "MATH101",  CourseName = "Calculus I",                   Description = "Limits, derivatives, and introductory integrals",                 Credits = 4, Capacity = 35, FacultyId = faculties[1].UserId, StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now.AddMonths(2), Status = "Active" },
                new Course { CourseCode = "MATH201",  CourseName = "Linear Algebra",               Description = "Vectors, matrices, linear transformations, eigenvalues",         Credits = 3, Capacity = 30, FacultyId = faculties[1].UserId, StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(5), Status = "Active" },
                new Course { CourseCode = "PHY101",   CourseName = "Physics I",                    Description = "Mechanics, thermodynamics, and wave motion",                    Credits = 4, Capacity = 35, FacultyId = faculties[2].UserId, StartDate = DateTime.Now.AddMonths(-5), EndDate = DateTime.Now.AddMonths(3), Status = "Active" },
                new Course { CourseCode = "BUS101",   CourseName = "Intro to Business",            Description = "Fundamentals of business management and entrepreneurship",       Credits = 3, Capacity = 40, FacultyId = faculties[4].UserId, StartDate = DateTime.Now.AddMonths(-4), EndDate = DateTime.Now.AddMonths(4), Status = "Active" },
                new Course { CourseCode = "BUS201",   CourseName = "Marketing Principles",         Description = "Marketing strategies, consumer behavior, and digital marketing", Credits = 3, Capacity = 35, FacultyId = faculties[4].UserId, StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now.AddMonths(5), Status = "Active" },
            };
            context.Courses.AddRange(courses);
            context.SaveChanges();

            // ======================== ENROLLMENTS + GRADES ========================
            var studentList = context.Students.ToList();
            var courseList = context.Courses.ToList();
            var grades = new[] { "A", "A", "B", "B", "B", "C", "C", "D", "F" };

            var enrollments = new List<Enrollment>();

            // Each student enrolls in 3-5 courses
            var courseMap = courseList.ToDictionary(c => c.CourseCode);

            var enrollMap = new (string student, string[] courses)[]
            {
                ("john.doe",        new[] { "CS101", "CS201", "MATH101", "PHY101" }),
                ("jane.smith",      new[] { "CS101", "CS201", "CS301", "MATH101" }),
                ("michael.brown",   new[] { "MATH101", "MATH201", "PHY101" }),
                ("emily.davis",     new[] { "PHY101", "MATH101", "CS101" }),
                ("daniel.wilson",   new[] { "CS101", "CS201", "CS301", "BUS101" }),
                ("sarah.anderson",  new[] { "BUS101", "BUS201", "MATH101" }),
                ("chris.taylor",    new[] { "MATH101", "MATH201", "CS101" }),
                ("ashley.thomas",   new[] { "CS101", "CS201", "PHY101" }),
                ("james.jackson",   new[] { "PHY101", "MATH201", "BUS101" }),
                ("rachel.white",    new[] { "BUS101", "BUS201", "CS101" }),
                ("kevin.harris",    new[] { "CS101", "CS201", "CS301" }),
                ("lisa.martin",     new[] { "MATH101", "MATH201", "BUS201" }),
                ("brian.garcia",    new[] { "PHY101", "MATH201", "BUS101" }),
                ("nicole.clark",    new[] { "BUS101", "BUS201", "MATH101" }),
                ("alex.rodriguez",  new[] { "CS101", "CS301", "PHY101" }),
            };

            foreach (var entry in enrollMap)
            {
                var student = studentList.First(s => s.Username == entry.student);
                foreach (var code in entry.courses)
                {
                    var course = courseMap[code];
                    bool hasGrade = rng.Next(100) < 75; // 75% already graded
                    enrollments.Add(new Enrollment
                    {
                        StudentId = student.UserId,
                        CourseId = course.CourseId,
                        EnrollmentDate = DateTime.Now.AddDays(-rng.Next(30, 120)),
                        Grade = hasGrade ? grades[rng.Next(grades.Length)] : "",
                        Status = "Active"
                    });
                }
            }
            context.Enrollments.AddRange(enrollments);
            context.SaveChanges();

            // ======================== ACADEMIC RECORDS ========================
            foreach (var student in studentList)
            {
                var studentEnrollments = context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == student.UserId && !string.IsNullOrEmpty(e.Grade))
                    .ToList();

                // Dùng cùng công thức GPA có trọng số tín chỉ như khi cập nhật điểm lúc chạy
                double gpa = AcademicRecord.CalculateGPA(studentEnrollments);

                // Môn trượt (F) không tính vào tín chỉ tích lũy
                int credits = studentEnrollments
                    .Where(e => e.Grade != "F")
                    .Sum(e => e.Course?.Credits ?? 0);

                context.AcademicRecords.Add(new AcademicRecord
                {
                    StudentId = student.UserId,
                    GPA = gpa,
                    TotalCreditsCompleted = credits,
                    YearStarted = student.EnrollmentDate.Year
                });
            }
            context.SaveChanges();

            // ======================== ASSIGNMENTS (per course) ========================
            var allAssignments = new List<Assignment>();
            foreach (var course in courseList)
            {
                var a1 = new Assignment { Title = $"{course.CourseCode} Midterm Exam",     Description = "Midterm examination covering first half of the course",        DueDate = DateTime.Now.AddDays(-30), CourseId = course.CourseId };
                var a2 = new Assignment { Title = $"{course.CourseCode} Homework 1",        Description = "Problem set on fundamental concepts",                           DueDate = DateTime.Now.AddDays(-15), CourseId = course.CourseId };
                var a3 = new Assignment { Title = $"{course.CourseCode} Final Project",     Description = "Comprehensive project demonstrating course mastery",             DueDate = DateTime.Now.AddDays(15),  CourseId = course.CourseId };
                allAssignments.AddRange(new[] { a1, a2, a3 });
            }
            context.Assignments.AddRange(allAssignments);
            context.SaveChanges();

            // ======================== SUBMISSIONS (some students submit) ========================
            var allSubmissions = new List<Submission>();
            var assignmentList = context.Assignments.Include(a => a.Course).ToList();

            foreach (var enrollment in enrollments)
            {
                // Student submits for assignments of their enrolled courses (80% chance)
                var courseAssignments = assignmentList.Where(a => a.CourseId == enrollment.CourseId).ToList();
                foreach (var assignment in courseAssignments)
                {
                    if (rng.Next(100) < 80)
                    {
                        bool alreadyExists = allSubmissions.Any(s =>
                            s.AssignmentId == assignment.AssignmentId && s.StudentId == enrollment.StudentId);
                        if (!alreadyExists)
                        {
                            bool isGraded = rng.Next(100) < 50;
                            allSubmissions.Add(new Submission
                            {
                                AssignmentId = assignment.AssignmentId,
                                StudentId = enrollment.StudentId,
                                FileName = $"submission_{enrollment.StudentId}_{assignment.AssignmentId}.pdf",
                                FilePath = $"/uploads/submissions/submission_{enrollment.StudentId}_{assignment.AssignmentId}.pdf",
                                SubmittedAt = DateTime.Now.AddDays(-rng.Next(5, 40)),
                                Grade = isGraded ? grades[rng.Next(grades.Length)] : "",
                                Feedback = isGraded ? "Good work!" : "",
                                IsGraded = isGraded
                            });
                        }
                    }
                }
            }
            context.Submissions.AddRange(allSubmissions);
            context.SaveChanges();

            // ======================== ATTENDANCE (last 4 weeks) ========================
            var allAttendance = new List<Attendance>();
            foreach (var course in courseList)
            {
                var enrolledStudents = enrollments.Where(e => e.CourseId == course.CourseId).ToList();
                // 8 class sessions over the last 4 weeks (2 per week)
                for (int week = 0; week < 4; week++)
                {
                    for (int day = 0; day < 2; day++)
                    {
                        var sessionDate = DateTime.Now.AddDays(-((3 - week) * 7 + day * 3));
                        foreach (var enrollment in enrolledStudents)
                        {
                            allAttendance.Add(new Attendance
                            {
                                CourseId = course.CourseId,
                                StudentId = enrollment.StudentId,
                                Date = sessionDate,
                                IsPresent = rng.Next(100) < 85 // 85% attendance rate
                            });
                        }
                    }
                }
            }
            context.Attendances.AddRange(allAttendance);
            context.SaveChanges();

            // ======================== COURSE REVIEWS ========================
            var allReviews = new List<CourseReview>();
            var reviewComments = new[]
            {
                "Great course, learned a lot!",
                "The professor explains concepts very clearly.",
                "Challenging but rewarding.",
                "Good material, could use more hands-on exercises.",
                "Excellent course, highly recommend!",
                "The pace was just right.",
                "Could be improved with more real-world examples.",
                "Very informative and well-structured.",
                "Difficult but the professor is supportive.",
                "Enjoyed this course a lot!"
            };

            foreach (var enrollment in enrollments.Where(e => !string.IsNullOrEmpty(e.Grade)))
            {
                if (rng.Next(100) < 60) // 60% leave a review
                {
                    bool exists = allReviews.Any(r =>
                        r.StudentId == enrollment.StudentId && r.CourseId == enrollment.CourseId);
                    if (!exists)
                    {
                        allReviews.Add(new CourseReview
                        {
                            StudentId = enrollment.StudentId,
                            CourseId = enrollment.CourseId,
                            Rating = rng.Next(3, 6), // 3-5 stars
                            Comment = reviewComments[rng.Next(reviewComments.Length)],
                            ReviewDate = DateTime.Now.AddDays(-rng.Next(1, 30))
                        });
                    }
                }
            }
            context.CourseReviews.AddRange(allReviews);
            context.SaveChanges();

            Console.WriteLine("Database seeded successfully!");
        }
    }
}
