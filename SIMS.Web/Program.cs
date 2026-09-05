using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;
using SIMS.Web.Repositories;
using SIMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC — bật kiểm tra chống CSRF cho MỌI POST/PUT/DELETE.
// Các form đều dùng tag helper asp-action nên token đã được sinh sẵn.
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Add DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session — cookie phiên là "vé" đăng nhập duy nhất của ứng dụng nên phải siết lại
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;                                 // JS không đọc được
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Secure khi chạy HTTPS, vẫn hoạt động khi dev HTTP
    options.Cookie.SameSite = SameSiteMode.Strict;                  // chặn gửi kèm từ site khác
    options.Cookie.IsEssential = true;                       // không bị cookie-consent loại bỏ
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Register Repositories
builder.Services.AddScoped<IRepository<Student>, StudentRepository>();
builder.Services.AddScoped<IRepository<Course>, CourseRepository>();
builder.Services.AddScoped<CourseReviewRepository>();

// Register Services
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<CourseReviewService>();
builder.Services.AddScoped<FacultyService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<AccessControlService>();

// CSV bulk import (xử lý dữ liệu lớn). Đăng ký parser qua interface để giữ DIP/OCP:
// muốn import thêm loại dữ liệu khác chỉ cần thêm parser mới, không sửa service import.
builder.Services.AddScoped<SIMS.Web.Services.CsvImport.ICsvRowParser<SIMS.Web.Services.CsvImport.StudentCsvRecord>,
                           SIMS.Web.Services.CsvImport.StudentCsvRowParser>();
builder.Services.AddScoped<SIMS.Web.Services.CsvImport.StudentCsvImportService>();

var app = builder.Build();

// Initialize Database with sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

// Di dời bài nộp cũ ra khỏi wwwroot.
// Trước đây file nằm trong wwwroot nên UseStaticFiles phục vụ công khai cho bất kỳ ai
// biết đường dẫn — kể cả người chưa đăng nhập. Từ nay chỉ tải qua Assignment/Download.
{
    var legacyDir = Path.Combine(app.Environment.WebRootPath, "uploads", "submissions");
    var secureDir = Path.Combine(app.Environment.ContentRootPath, "App_Data", "submissions");

    if (Directory.Exists(legacyDir))
    {
        Directory.CreateDirectory(secureDir);
        foreach (var source in Directory.GetFiles(legacyDir))
        {
            var target = Path.Combine(secureDir, Path.GetFileName(source));
            try
            {
                if (!File.Exists(target))
                    File.Move(source, target);
                else
                    File.Delete(source); // đã di dời trước đó, xóa bản công khai còn sót
            }
            catch (IOException)
            {
                // File đang bị khóa — bỏ qua, lần khởi động sau sẽ thử lại
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    // Không trỏ tới /Home/Error vì HomeController không tồn tại -> lỗi sẽ nuốt mất chính nó.
    app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "text/html";
        await ctx.Response.WriteAsync(
            "<h1>Something went wrong</h1><p>Please try again or contact the administrator.</p>");
    }));
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
