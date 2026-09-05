using Moq;
using SIMS.Web.Data;
using SIMS.Web.Models;
using SIMS.Web.Repositories;
using SIMS.Web.Services;
using SIMS.Web.Tests.Fixtures;
using Xunit;

namespace SIMS.Web.Tests.Tests
{
    /// <summary>
    /// Student Tests - Kiểm thử các chức năng sinh viên
    /// Bao gồm: CourseService, CourseReviewService, NotificationService
    /// </summary>
    public class StudentTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;

        public StudentTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        // ===================================================================
        // COURSE SERVICE TESTS (sử dụng Moq Repository)
        // ===================================================================

        [Fact]
        public async Task CourseService_GetAll_ReturnsCourses()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Course>>();
            var courses = new List<Course>
            {
                new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Intro to Programming", Credits = 3 },
                new Course { CourseId = 2, CourseCode = "CS201", CourseName = "Data Structures", Credits = 4 }
            };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(courses);

            var service = new CourseService(mockRepo.Object);

            // Act
            var result = await service.GetAllCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("CS101", result[0].CourseCode);
            Assert.Equal("CS201", result[1].CourseCode);
        }

        [Fact]
        public async Task CourseService_GetById_ReturnsCourse()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Course>>();
            var course = new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Intro to Programming", Credits = 3 };
            mockRepo.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(course);

            var service = new CourseService(mockRepo.Object);

            // Act
            var result = await service.GetCourseAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CS101", result.CourseCode);
            Assert.Equal("Intro to Programming", result.CourseName);
        }

        [Fact]
        public async Task CourseService_GetById_InvalidId_ReturnsNull()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Course>>();
            mockRepo.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((Course?)null);

            var service = new CourseService(mockRepo.Object);

            // Act
            var result = await service.GetCourseAsync("invalid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CourseService_CreateCourse_AddsCourse()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Course>>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);

            var service = new CourseService(mockRepo.Object);
            var newCourse = new Course
            {
                CourseCode = "CS301",
                CourseName = "Algorithms",
                Credits = 4,
                Capacity = 20
            };

            // Act
            await service.CreateCourseAsync(newCourse);

            // Assert
            mockRepo.Verify(r => r.AddAsync(It.Is<Course>(
                c => c.CourseCode == "CS301" && c.CourseName == "Algorithms")),
                Times.Once);
        }

        // ===================================================================
        // COURSE REVIEW SERVICE TESTS (sử dụng InMemory Database + Real Repository)
        // ===================================================================

        [Fact]
        public async Task CourseReviewService_GetByCourse_ReturnsReviews()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var repo = new CourseReviewRepository(context);
            var service = new CourseReviewService(repo);

            // Act
            var result = await service.GetReviewsByCourseAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            // Ordered by ReviewDate descending - review 2 is more recent
            Assert.Equal(4, result[0].Rating);
        }

        [Fact]
        public async Task CourseReviewService_GetByStudentAndCourse_ReturnsReview()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var repo = new CourseReviewRepository(context);
            var service = new CourseReviewService(repo);

            // Act
            var result = await service.GetReviewByStudentAndCourseAsync(10, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Rating);
            Assert.Equal(10, result.StudentId);
        }

        [Fact]
        public async Task CourseReviewService_CreateOrUpdate_CreatesNew()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var repo = new CourseReviewRepository(context);
            var service = new CourseReviewService(repo);

            var newReview = new CourseReview
            {
                StudentId = 12,
                CourseId = 1,
                Rating = 3,
                Comment = "Average course"
            };

            // Act
            await service.CreateOrUpdateReviewAsync(newReview);

            // Assert
            var saved = context.CourseReviews
                .FirstOrDefault(cr => cr.StudentId == 12 && cr.CourseId == 1);
            Assert.NotNull(saved);
            Assert.Equal(3, saved.Rating);
        }

        [Fact]
        public async Task CourseReviewService_DeleteReview_Success()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var repo = new CourseReviewRepository(context);
            var service = new CourseReviewService(repo);

            // Act
            await service.DeleteReviewAsync("1");

            // Assert
            var deleted = context.CourseReviews.Find(1);
            Assert.Null(deleted);
            Assert.Single(context.CourseReviews); // Only review 2 remains
        }

        [Fact]
        public async Task CourseReviewService_GetAverageRating_ReturnsCorrect()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var repo = new CourseReviewRepository(context);
            var service = new CourseReviewService(repo);

            // Act
            var result = await service.GetAverageRatingAsync(1);

            // Assert
            Assert.Equal(4.5, result);
        }

        // ===================================================================
        // NOTIFICATION SERVICE TESTS (sử dụng InMemory Database)
        // ===================================================================

        [Fact]
        public async Task NotificationService_Create_Success()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act
            await service.CreateAsync(10, "Test Title", "Test Message", "General");

            // Assert
            var notification = context.Notifications
                .FirstOrDefault(n => n.UserId == 10 && n.Title == "Test Title");
            Assert.NotNull(notification);
            Assert.Equal("Test Message", notification.Message);
            Assert.False(notification.IsRead);
        }

        [Fact]
        public async Task NotificationService_GetByUser_ReturnsList()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.GetByUserAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // 2 notifications for user 10
            Assert.All(result, n => Assert.Equal(10, n.UserId));
            // Ordered by CreatedAt descending
            Assert.True(result[0].CreatedAt >= result[1].CreatedAt);
        }

        [Fact]
        public async Task NotificationService_GetUnreadCount_ReturnsCorrect()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync(10);

            // Assert
            Assert.Equal(1, result); // Only 1 unread notification for user 10
        }

        [Fact]
        public async Task NotificationService_MarkAsRead_UpdatesStatus()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act - notification 1 belongs to user 10
            await service.MarkAsReadAsync(1, 10);

            // Assert
            var notification = context.Notifications.Find(1);
            Assert.NotNull(notification);
            Assert.True(notification.IsRead);
        }

        [Fact]
        public async Task NotificationService_MarkAsRead_IgnoresOtherUsersNotification()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act - user 11 tries to mark user 10's notification as read
            await service.MarkAsReadAsync(1, 11);

            // Assert - it stays unread
            var notification = context.Notifications.Find(1);
            Assert.NotNull(notification);
            Assert.False(notification.IsRead);
        }

        [Fact]
        public async Task NotificationService_MarkAllAsRead_UpdatesAll()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var service = new NotificationService(context);

            // Act
            await service.MarkAllAsReadAsync(10);

            // Assert
            var unreadCount = context.Notifications
                .Count(n => n.UserId == 10 && !n.IsRead);
            Assert.Equal(0, unreadCount);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
