using Microsoft.AspNetCore.Hosting;
using Moq;
using SIMS.Web.Data;
using SIMS.Web.Models;
using SIMS.Web.Services;
using SIMS.Web.Tests.Fixtures;
using Xunit;

namespace SIMS.Web.Tests.Tests
{
    /// <summary>
    /// Faculty Tests - Kiểm thử các chức năng giảng viên
    /// Bao gồm: FacultyService, AttendanceService, AssignmentService
    /// </summary>
    public class FacultyTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;

        public FacultyTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        // ===================================================================
        // FACULTY SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task FacultyService_GetAll_ReturnsFaculties()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new FacultyService(context);

            // Act
            var result = await service.GetAllFacultiesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, f => f.FirstName == "Robert");
            Assert.Contains(result, f => f.FirstName == "Emily");
        }

        [Fact]
        public async Task FacultyService_GetByDepartment_FiltersCorrectly()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new FacultyService(context);

            // Act
            var result = await service.GetFacultiesByDepartmentAsync("Computer Science");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Robert", result[0].FirstName);
            Assert.Equal("Computer Science", result[0].Department);
        }

        [Fact]
        public async Task FacultyService_GetById_ReturnsFaculty()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new FacultyService(context);

            // Act
            var result = await service.GetFacultyAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Robert", result.FirstName);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal("F001", result.FacultyId);
        }

        // ===================================================================
        // ATTENDANCE SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task AttendanceService_GetEnrolledStudents_ReturnsList()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);

            // Act - Get students enrolled in course 1
            var result = await service.GetEnrolledStudentsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(1, e.CourseId));
        }

        [Fact]
        public async Task AttendanceService_SaveAttendance_CreatesRecord()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);
            var date = DateTime.Now.Date;
            var attendanceData = new Dictionary<int, bool>
            {
                { 10, true },   // John present
                { 11, false }   // Jane absent
            };

            // Act
            await service.SaveAttendanceAsync(1, date, attendanceData);

            // Assert
            var records = context.Attendances
                .Where(a => a.CourseId == 1 && a.Date.Date == date)
                .ToList();
            Assert.Equal(2, records.Count);

            var johnRecord = records.FirstOrDefault(a => a.StudentId == 10);
            Assert.NotNull(johnRecord);
            Assert.True(johnRecord.IsPresent);

            var janeRecord = records.FirstOrDefault(a => a.StudentId == 11);
            Assert.NotNull(janeRecord);
            Assert.False(janeRecord.IsPresent);
        }

        [Fact]
        public async Task AttendanceService_SaveAttendance_UpdatesExisting()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);
            var date = DateTime.Now.AddDays(-7).Date; // Same date as existing record
            var attendanceData = new Dictionary<int, bool>
            {
                { 10, false }   // Was present, now absent
            };

            // Act
            await service.SaveAttendanceAsync(1, date, attendanceData);

            // Assert
            var record = context.Attendances
                .FirstOrDefault(a => a.CourseId == 1 && a.StudentId == 10 && a.Date.Date == date);
            Assert.NotNull(record);
            Assert.False(record.IsPresent); // Updated from true to false
        }

        [Fact]
        public async Task AttendanceService_GetByCourse_ReturnsRecords()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);

            // Act
            var result = await service.GetAttendanceByCourseAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // 3 records for course 1
            // Ordered by date descending
            Assert.True(result[0].Date >= result[1].Date);
        }

        [Fact]
        public async Task AttendanceService_GetSummary_ReturnsStats()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);

            // Act
            var result = await service.GetAttendanceSummaryAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(10));
            Assert.True(result.ContainsKey(11));

            // Student 10: 2 present (attendance 1, 3)
            Assert.Equal(2, result[10].Present);
            Assert.Equal(0, result[10].Absent);

            // Student 11: 0 present, 1 absent
            Assert.Equal(0, result[11].Present);
            Assert.Equal(1, result[11].Absent);
        }

        // ===================================================================
        // ASSIGNMENT SERVICE TESTS (sử dụng InMemory Database + Moq)
        // ===================================================================

        [Fact]
        public async Task AssignmentService_CreateAssignment_Success()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var notificationService = new NotificationService(context);
            var service = new AssignmentService(context, mockEnv.Object, notificationService);

            var assignment = new Assignment
            {
                Title = "Final Project",
                Description = "Build a web app",
                CourseId = 1,
                DueDate = DateTime.Now.AddDays(30)
            };

            // Act
            await service.CreateAssignmentAsync(assignment);

            // Assert
            var saved = context.Assignments.FirstOrDefault(a => a.Title == "Final Project");
            Assert.NotNull(saved);
            Assert.Equal(1, saved.CourseId);

            // Check notifications were sent to enrolled students
            var notifications = context.Notifications
                .Where(n => n.Type == "Assignment" && n.Title == "New Assignment")
                .ToList();
            Assert.Equal(2, notifications.Count); // 2 students enrolled in course 1
        }

        [Fact]
        public async Task AssignmentService_GetByCourse_ReturnsAssignments()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var notificationService = new NotificationService(context);
            var service = new AssignmentService(context, mockEnv.Object, notificationService);

            // Act
            var result = await service.GetByCourseAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 2 assignments for course 1
            // Ordered by DueDate descending
            Assert.True(result[0].DueDate >= result[1].DueDate);
        }

        [Fact]
        public async Task AssignmentService_GradeSubmission_Success()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var notificationService = new NotificationService(context);
            var service = new AssignmentService(context, mockEnv.Object, notificationService);

            // Act - Grade submission 1 with "A"
            await service.GradeSubmissionAsync(1, "A", "Excellent work!");

            // Assert
            var submission = context.Submissions.Find(1);
            Assert.NotNull(submission);
            Assert.Equal("A", submission.Grade);
            Assert.Equal("Excellent work!", submission.Feedback);
            Assert.True(submission.IsGraded);

            // Check notification was sent
            var notification = context.Notifications
                .FirstOrDefault(n => n.UserId == 10 && n.Type == "Grade" && n.Title == "Assignment Graded");
            Assert.NotNull(notification);
        }

        [Fact]
        public async Task AssignmentService_GetSubmissions_ReturnsList()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var notificationService = new NotificationService(context);
            var service = new AssignmentService(context, mockEnv.Object, notificationService);

            // Act
            var result = await service.GetSubmissionsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 2 submissions for assignment 1
            Assert.All(result, s => Assert.Equal(1, s.AssignmentId));
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
