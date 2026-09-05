using Moq;
using SIMS.Web.Models;
using SIMS.Web.Repositories;
using SIMS.Web.Services;
using SIMS.Web.Tests.Fixtures;
using SIMS.Web.ViewModels;
using Xunit;

namespace SIMS.Web.Tests.Tests
{
    /// <summary>
    /// Admin Tests - Kiểm thử các chức năng quản trị
    /// Bao gồm: StudentService, RoleService, EnrollmentService, DashboardService
    /// </summary>
    public class AdminTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;

        public AdminTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        // ===================================================================
        // STUDENT SERVICE TESTS (sử dụng Moq Repository)
        // ===================================================================

        [Fact]
        public async Task StudentService_GetAll_ReturnsStudents()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Student>>();
            var students = new List<Student>
            {
                new Student { UserId = 10, StudentId = "S001", FirstName = "John", LastName = "Doe" },
                new Student { UserId = 11, StudentId = "S002", FirstName = "Jane", LastName = "Smith" }
            };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(students);

            var service = new StudentService(mockRepo.Object);

            // Act
            var result = await service.GetAllStudentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].FirstName);
            Assert.Equal("Jane", result[1].FirstName);
        }

        [Fact]
        public async Task StudentService_GetById_ReturnsStudent()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Student>>();
            var student = new Student { UserId = 10, StudentId = "S001", FirstName = "John", LastName = "Doe" };
            mockRepo.Setup(r => r.GetByIdAsync("10")).ReturnsAsync(student);

            var service = new StudentService(mockRepo.Object);

            // Act
            var result = await service.GetStudentAsync("10");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
        }

        [Fact]
        public async Task StudentService_Register_AddsStudent()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Student>>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

            var service = new StudentService(mockRepo.Object);
            var newStudent = new Student
            {
                UserId = 20,
                StudentId = "S004",
                FirstName = "Test",
                LastName = "Student",
                AcademicProgram = "Computer Science"
            };

            // Act
            await service.RegisterStudentAsync(newStudent);

            // Assert
            mockRepo.Verify(r => r.AddAsync(It.Is<Student>(
                s => s.FirstName == "Test" && s.LastName == "Student")),
                Times.Once);
            Assert.Equal(DateTime.Now.Date, newStudent.EnrollmentDate.Date);
        }

        [Fact]
        public async Task StudentService_Delete_RemovesStudent()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Student>>();
            mockRepo.Setup(r => r.DeleteAsync("10")).Returns(Task.CompletedTask);

            var service = new StudentService(mockRepo.Object);

            // Act
            await service.DeleteStudentAsync("10");

            // Assert
            mockRepo.Verify(r => r.DeleteAsync("10"), Times.Once);
        }

        // ===================================================================
        // ROLE SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task RoleService_GetAllRoles_ReturnsRoles()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new RoleService(context);

            // Act
            var result = await service.GetAllRolesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.RoleName == "Admin");
            Assert.Contains(result, r => r.RoleName == "Faculty");
            Assert.Contains(result, r => r.RoleName == "Student");
        }

        [Fact]
        public async Task RoleService_GetRoleById_ReturnsRole()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new RoleService(context);

            // Act
            var result = await service.GetRoleByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.RoleName);
        }

        [Fact]
        public async Task RoleService_ChangeUserRole_RejectsCrossTypeChange()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new RoleService(context);

            // Act - try to turn a Student row (UserId=10) into a Faculty role.
            // EF cannot change the TPH discriminator, so this would leave a Student
            // record carrying the Faculty role - a broken account that 404s on login.
            var error = await service.ChangeUserRoleAsync(10, 2);

            // Assert - rejected, role unchanged
            Assert.NotNull(error);
            var user = context.Users.Find(10);
            Assert.NotNull(user);
            Assert.Equal(3, user.RoleId);
        }

        [Fact]
        public async Task RoleService_ChangeUserRole_AllowsMatchingType()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new RoleService(context);

            // Act - a Student row assigned the Student role is consistent
            var error = await service.ChangeUserRoleAsync(10, 3);

            // Assert
            Assert.Null(error);
            var user = context.Users.Find(10);
            Assert.NotNull(user);
            Assert.Equal(3, user.RoleId);
        }

        [Fact]
        public async Task RoleService_GetUsersByRole_ReturnsCorrectUsers()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new RoleService(context);

            // Act - Get all students (RoleId = 3)
            var result = await service.GetUsersByRoleAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, u => Assert.Equal(3, u.RoleId));
        }

        // ===================================================================
        // ENROLLMENT SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task EnrollmentService_AssignStudentToCourse_Success()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var notificationService = new NotificationService(context);
            var service = new EnrollmentService(context, notificationService);

            // Act - Assign student 12 to course 1
            await service.AssignStudentToCourseAsync("12", "1");

            // Assert
            var enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == 12 && e.CourseId == 1);
            Assert.NotNull(enrollment);
            Assert.Equal("Active", enrollment.Status);
            Assert.Equal("", enrollment.Grade);

            // Check notification was created
            var notification = context.Notifications
                .FirstOrDefault(n => n.UserId == 12 && n.Type == "Enrollment");
            Assert.NotNull(notification);
            Assert.Contains("Introduction to Programming", notification.Message);
        }

        [Fact]
        public async Task EnrollmentService_InputGrade_UpdatesGrade()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var notificationService = new NotificationService(context);
            var service = new EnrollmentService(context, notificationService);

            // Act - Input grade "A" for enrollment 4 (student 11, course 2)
            await service.InputGradeAsync("4", "A");

            // Assert
            var enrollment = context.Enrollments.Find(4);
            Assert.NotNull(enrollment);
            Assert.Equal("A", enrollment.Grade);

            // Check GPA was recalculated
            var record = context.AcademicRecords.FirstOrDefault(r => r.StudentId == 11);
            Assert.NotNull(record);
        }

        [Fact]
        public async Task EnrollmentService_GetEnrollmentsByStudent_ReturnsList()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var notificationService = new NotificationService(context);
            var service = new EnrollmentService(context, notificationService);

            // Act
            var result = await service.GetEnrollmentsByStudentAsync("10");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(10, e.StudentId));
        }

        // ===================================================================
        // DASHBOARD SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task DashboardService_GetDashboard_ReturnsCounts()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetDashboardAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalStudents);
            Assert.Equal(2, result.TotalFaculty);
            Assert.Equal(3, result.TotalCourses);
            Assert.Equal(3, result.ActiveCourses);
            Assert.Equal(5, result.TotalEnrollments);
        }

        [Fact]
        public async Task DashboardService_GetDashboard_GradeDistribution()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetDashboardAsync();

            // Assert
            Assert.Equal(1, result.GradeA);    // enrollment 1
            Assert.Equal(2, result.GradeB);    // enrollment 2, 3
            Assert.Equal(1, result.GradeC);    // enrollment 5
            Assert.Equal(4, result.TotalGraded); // A, B, B, C
        }

        [Fact]
        public async Task DashboardService_GetDashboard_AttendanceStats()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetDashboardAsync();

            // Assert
            Assert.Equal(4, result.TotalAttendanceRecords);
            Assert.Equal(3, result.PresentCount);  // 3 present out of 4
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
