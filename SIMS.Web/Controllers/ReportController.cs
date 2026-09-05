using System.Text;
using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Services;

namespace SIMS.Web.Controllers
{
    [Authorize("Admin")]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _reportService.GetReportsDashboardAsync();
            return View(dashboard);
        }

        // NOTE_EXPORT_CSV: Xử lý nút "Export CSV" của bảng Grade Report.
        // Lấy lại dữ liệu report -> ReportService.BuildGradeReportCsv() dựng chuỗi CSV
        // -> gắn BOM UTF-8 (để Excel đọc đúng dấu tiếng Việt) -> trả về file .csv cho trình duyệt tải.
        // Xuất báo cáo điểm theo khóa ra file CSV để tải về.
        public async Task<IActionResult> ExportGradesCsv()
        {
            var dashboard = await _reportService.GetReportsDashboardAsync();
            var csv = ReportService.BuildGradeReportCsv(dashboard);

            // Thêm BOM UTF-8 để Excel mở đúng tiếng Việt có dấu.
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            var fileName = $"grade-report-{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // NOTE_EXPORT_CSV: Xử lý nút "Export CSV" của bảng Attendance Report.
        // Cùng cơ chế như trên nhưng gọi ReportService.BuildAttendanceReportCsv().
        // Xuất báo cáo điểm danh theo khóa ra file CSV để tải về.
        public async Task<IActionResult> ExportAttendanceCsv()
        {
            var dashboard = await _reportService.GetReportsDashboardAsync();
            var csv = ReportService.BuildAttendanceReportCsv(dashboard);

            // Thêm BOM UTF-8 để Excel mở đúng tiếng Việt có dấu.
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            var fileName = $"attendance-report-{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}
