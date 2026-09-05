using Microsoft.EntityFrameworkCore;
using SIMS.Web.Models;

namespace SIMS.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AcademicRecord> AcademicRecords { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. TPH (Table Per Hierarchy) cho User, Student, Faculty, Administrator
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Student>("Student")
                .HasValue<Faculty>("Faculty")
                .HasValue<Administrator>("Administrator");

            // 2. Mối quan hệ User - Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade khi xóa Role

            // 3. Mối quan hệ Enrollment - Student (User)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // <<< QUAN TRỌNG: Ngắt vòng lặp cascade ở đây!

            // 4. Mối quan hệ Enrollment - Course
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Course thì tự xóa Enrollment (giữ lại 1 đường cascade này)

            // Mỗi sinh viên chỉ được đăng ký MỘT lần cho mỗi khóa học.
            // Chặn trùng tận gốc ở tầng DB (guard ở tầng service vẫn giữ để báo lỗi thân thiện).
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            // 5. Mối quan hệ AcademicRecord - Student
            modelBuilder.Entity<AcademicRecord>()
                .HasOne(a => a.Student)
                .WithOne(s => s.AcademicRecord)
                .HasForeignKey<AcademicRecord>(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade ở đây
                                                    // Assignment - Course
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)  // ← phải trỏ đúng vào collection
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission - Assignment
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission - Student
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mỗi student chỉ nộp 1 lần cho mỗi assignment
            modelBuilder.Entity<Submission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();

            // Attendance - Course
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attendance - Student
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mỗi student chỉ có 1 record điểm danh mỗi ngày mỗi course
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.CourseId, a.StudentId, a.Date })
                .IsUnique();

            // CourseReview - Course
            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // CourseReview - Student
            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.Reviews)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mỗi student chỉ có 1 review cho mỗi course
            modelBuilder.Entity<CourseReview>()
                .HasIndex(cr => new { cr.StudentId, cr.CourseId })
                .IsUnique();
        }
    }
}
