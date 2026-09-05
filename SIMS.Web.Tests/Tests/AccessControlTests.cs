using SIMS.Web.Services;
using SIMS.Web.Tests.Fixtures;
using Xunit;

namespace SIMS.Web.Tests.Tests
{
    /// <summary>
    /// Access Control Tests — Kiểm thử phân quyền ở mức bản ghi.
    ///
    /// Dữ liệu mẫu:
    ///   - Giảng viên 2 (dr.smith)   phụ trách lớp 1 (CS101) và 2 (CS201)
    ///   - Giảng viên 3 (dr.johnson) phụ trách lớp 3 (MATH101)
    ///   - Sinh viên 10 học lớp 1 và 3
    ///   - Sinh viên 11 học lớp 1 và 2
    ///   - Sinh viên 12 CHỈ học lớp 3  -> không phải sinh viên của dr.smith
    /// </summary>
    public class AccessControlTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;

        public AccessControlTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        // ===================================================================
        // QUY TẮC CHÍNH: giảng viên chỉ thấy sinh viên trong lớp mình dạy
        // ===================================================================

        [Fact]
        public async Task Faculty_CannotSeeStudentFromAnotherTeachersClass()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // dr.smith (2) không dạy lớp nào của sinh viên 12
            var result = await access.FacultyTeachesStudentAsync(2, 12);

            Assert.False(result);
        }

        [Fact]
        public async Task Faculty_CanSeeOwnStudent()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // Sinh viên 10 học CS101 do dr.smith phụ trách
            Assert.True(await access.FacultyTeachesStudentAsync(2, 10));
            // Sinh viên 12 học MATH101 do dr.johnson phụ trách
            Assert.True(await access.FacultyTeachesStudentAsync(3, 12));
        }

        [Fact]
        public async Task StudentIdsTaughtBy_OnlyReturnsOwnStudents()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            var smithStudents = await access.StudentIdsTaughtByAsync(2);

            Assert.Contains(10, smithStudents);
            Assert.Contains(11, smithStudents);
            Assert.DoesNotContain(12, smithStudents);   // sinh viên của giảng viên khác
        }

        [Fact]
        public async Task CourseIdsTaughtBy_OnlyReturnsOwnCourses()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            var smithCourses = await access.CourseIdsTaughtByAsync(2);

            Assert.Equal(new[] { 1, 2 }, smithCourses.OrderBy(c => c).ToArray());
        }

        // ===================================================================
        // QUYỀN TRÊN LỚP / ĐIỂM / BÀI TẬP / BÀI NỘP
        // ===================================================================

        [Fact]
        public async Task FacultyOwnsCourse_FalseForOtherTeachersCourse()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            Assert.True(await access.FacultyOwnsCourseAsync(2, 1));    // lớp của mình
            Assert.False(await access.FacultyOwnsCourseAsync(2, 3));   // lớp của dr.johnson
        }

        [Fact]
        public async Task FacultyOwnsEnrollment_FalseForOtherTeachersEnrollment()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // Enrollment 5 = sinh viên 12 trong lớp 3 (của dr.johnson)
            Assert.False(await access.FacultyOwnsEnrollmentAsync(2, 5));
            Assert.True(await access.FacultyOwnsEnrollmentAsync(3, 5));
        }

        [Fact]
        public async Task FacultyOwnsAssignment_FalseForOtherTeachersAssignment()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // Assignment 1 thuộc lớp 1 (dr.smith)
            Assert.True(await access.FacultyOwnsAssignmentAsync(2, 1));
            Assert.False(await access.FacultyOwnsAssignmentAsync(3, 1));
        }

        [Fact]
        public async Task FacultyOwnsSubmission_FalseForOtherTeachersSubmission()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // Submission 1 -> assignment 1 -> lớp 1 (dr.smith)
            Assert.True(await access.FacultyOwnsSubmissionAsync(2, 1));
            Assert.False(await access.FacultyOwnsSubmissionAsync(3, 1));
        }

        // ===================================================================
        // QUYỀN CỦA SINH VIÊN
        // ===================================================================

        [Fact]
        public async Task StudentEnrolledInCourse_FalseForCourseNotTaken()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            Assert.True(await access.StudentEnrolledInCourseAsync(12, 3));
            Assert.False(await access.StudentEnrolledInCourseAsync(12, 1));
        }

        [Fact]
        public async Task StudentEnrolledInAssignmentCourse_FalseForOtherClassAssignment()
        {
            using var context = _fixture.CreateContext();
            var access = new AccessControlService(context);

            // Assignment 1 thuộc lớp 1; sinh viên 12 không học lớp 1
            Assert.False(await access.StudentEnrolledInAssignmentCourseAsync(12, 1));
            Assert.True(await access.StudentEnrolledInAssignmentCourseAsync(10, 1));
        }

        // ===================================================================
        // ĐIỂM DANH: không ghi được cho sinh viên ngoài lớp
        // ===================================================================

        [Fact]
        public async Task SaveAttendance_RejectsStudentNotEnrolledInCourse()
        {
            using var context = _fixture.CreateContext();
            var service = new AttendanceService(context);
            var date = DateTime.Today;

            // Sinh viên 12 không học lớp 1 -> phải bị loại
            var rejected = await service.SaveAttendanceAsync(1, date,
                new Dictionary<int, bool> { { 10, true }, { 12, true } });

            Assert.Equal(1, rejected);
            Assert.DoesNotContain(context.Attendances,
                a => a.CourseId == 1 && a.StudentId == 12 && a.Date.Date == date.Date);
            Assert.Contains(context.Attendances,
                a => a.CourseId == 1 && a.StudentId == 10 && a.Date.Date == date.Date);
        }

        public void Dispose() => _fixture.Dispose();
    }
}
