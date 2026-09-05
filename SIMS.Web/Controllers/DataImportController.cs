using System.Text;
using Microsoft.AspNetCore.Mvc;
using SIMS.Web.Helpers;
using SIMS.Web.Services.CsvImport;

namespace SIMS.Web.Controllers
{
    // Nhập dữ liệu lớn từ CSV. Chỉ admin được dùng vì thao tác này tạo hàng loạt tài khoản.
    [Authorize("Admin")]
    public class DataImportController : Controller
    {
        private readonly StudentCsvImportService _importService;
        private readonly ICsvRowParser<StudentCsvRecord> _parser;

        // Chặn file quá lớn để tránh một upload lỗi/độc hại làm nghẽn server (50 MB ~ vài trăm nghìn dòng).
        private const long MaxFileSizeBytes = 50 * 1024 * 1024;

        public DataImportController(
            StudentCsvImportService importService,
            ICsvRowParser<StudentCsvRecord> parser)
        {
            _importService = importService;
            _parser = parser;
        }

        [HttpGet]
        public IActionResult Index() => View();

        // NOTE_UPLOAD_FILE: Xử lý file người dùng upload lên.
        // Nhận IFormFile từ form, kiểm tra: file rỗng? đúng đuôi .csv? có vượt 50MB?
        // Nếu hợp lệ thì mở stream đọc thẳng (không nạp cả file vào RAM) rồi đưa vào service nhập liệu.
        [HttpPost]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> Import(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn một file CSV.";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "File phải có định dạng .csv.";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > MaxFileSizeBytes)
            {
                TempData["Error"] = "File vượt quá giới hạn 50 MB.";
                return RedirectToAction(nameof(Index));
            }

            // NOTE_IMPORT_DATA: Nút "Import" bấm xong sẽ chạy tới đây để NHẬP dữ liệu vào hệ thống.
            // Truyền thẳng stream của file vào service — KHÔNG đọc toàn bộ vào bộ nhớ trước,
            // đúng tinh thần xử lý dữ liệu lớn theo luồng. Kết quả (số dòng ok/lỗi) trả về view.
            await using var stream = file.OpenReadStream();
            var result = await _importService.ImportAsync(stream);

            return View(nameof(Index), result);
        }

        // NOTE_DOWNLOAD_TEMPLATE: Xử lý nút "Download template".
        // Tải file CSV mẫu (header + vài dòng ví dụ). Header lấy trực tiếp từ parser
        // để mẫu luôn khớp với cột mà hệ thống đang mong đợi.
        [HttpGet]
        public IActionResult Template()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", _parser.ExpectedHeader));
            sb.AppendLine("Alice,Nguyen,alice.nguyen,alice.nguyen@student.sims.edu,,2004-03-12,555-1001,12 Le Loi,Da Nang,Computer Science");
            sb.AppendLine("Bao,Tran,bao.tran,bao.tran@student.sims.edu,,2003-11-05,555-1002,\"34 Tran Phu, Q1\",Ho Chi Minh,Business");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "students-template.csv");
        }
    }
}
