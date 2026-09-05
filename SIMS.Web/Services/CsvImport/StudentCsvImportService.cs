using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SIMS.Web.Data;
using SIMS.Web.Models;

namespace SIMS.Web.Services.CsvImport
{
    // Nhập một tập DỮ LIỆU LỚN sinh viên từ file CSV vào SQL Server.
    //
    // Hai kỹ thuật cốt lõi để xử lý được file lớn mà RAM không phình:
    //   1) ĐỌC THEO LUỒNG (streaming): đọc từng dòng bằng StreamReader thay vì nạp cả file.
    //   2) GHI THEO LÔ (batching): gom BatchSize dòng rồi SaveChanges một lần, sau đó xoá
    //      change-tracker để EF không giữ tham chiếu tới các entity đã ghi -> bộ nhớ phẳng.
    public class StudentCsvImportService
    {
        private readonly AppDbContext _context;
        private readonly AuthenticationService _authService;
        private readonly ICsvRowParser<StudentCsvRecord> _parser;

        // 500 là điểm cân bằng: đủ lớn để giảm số lần round-trip xuống DB,
        // đủ nhỏ để một lô không giữ quá nhiều entity trong bộ nhớ.
        private const int BatchSize = 500;

        public StudentCsvImportService(
            AppDbContext context,
            AuthenticationService authService,
            ICsvRowParser<StudentCsvRecord> parser)
        {
            _context = context;
            _authService = authService;
            _parser = parser;
        }

        // NOTE_IMPORT_PROCESS: Lõi nhập dữ liệu vào hệ thống.
        // Đọc CSV theo từng dòng (streaming) -> parse -> chống trùng username -> gom lô 500 dòng
        // -> ghi xuống SQL Server. Nhờ đọc luồng + ghi lô nên xử lý được file rất lớn.
        public async Task<CsvImportResult> ImportAsync(Stream csvStream, CancellationToken ct = default)
        {
            var result = new CsvImportResult();
            var stopwatch = Stopwatch.StartNew();

            var studentRole = await _authService.GetRoleByNameAsync("Student");
            int roleId = studentRole?.RoleId ?? 3;
            int currentYear = DateTime.Now.Year;

            // Nạp sẵn toàn bộ username đang có vào HashSet -> kiểm tra trùng O(1) trong bộ nhớ,
            // thay vì bắn một câu query xuống DB cho từng dòng (sẽ rất chậm với file lớn).
            // Add() trả về false nếu đã tồn tại, nên set này chống trùng được cả với DB
            // lẫn với các dòng lặp bên trong chính file.
            var knownUsernames = new HashSet<string>(
                await _context.Users.Select(u => u.Username).ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);

            // Băm BCrypt rất chậm (cố ý). Cache theo mật khẩu để không băm lại cùng một chuỗi
            // nhiều lần — với file mà mọi dòng dùng mật khẩu mặc định thì chỉ băm đúng 1 lần.
            var hashCache = new Dictionary<string, string>();

            using var reader = new StreamReader(csvStream);

            // Dòng 1 là header -> bỏ qua, chỉ dùng để căn số thứ tự dòng cho thông báo lỗi.
            var header = await reader.ReadLineAsync();
            int lineNumber = 1;

            var batch = new List<Student>(BatchSize);
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                result.TotalRows++;

                // NOTE_VALIDATE_ERROR: Mỗi dòng được parse & kiểm tra. Nếu sai (thiếu cột,
                // email/ngày sai...) thì KHÔNG dừng cả file — chỉ ghi lại "dòng số mấy, sai gì"
                // qua result.AddError rồi bỏ qua dòng đó, các dòng đúng vẫn được nhập bình thường.
                var fields = CsvLineSplitter.Split(line);
                var parsed = _parser.Parse(fields, lineNumber);
                if (!parsed.Success)
                {
                    result.AddError(lineNumber, parsed.Error!);
                    continue;
                }

                var record = parsed.Value!;

                if (!knownUsernames.Add(record.Username))
                {
                    result.AddError(lineNumber,
                        $"Username '{record.Username}' đã tồn tại hoặc bị lặp trong file.");
                    continue;
                }

                if (!hashCache.TryGetValue(record.Password, out var passwordHash))
                {
                    passwordHash = _authService.HashPassword(record.Password);
                    hashCache[record.Password] = passwordHash;
                }

                batch.Add(new Student
                {
                    Username = record.Username,
                    Email = record.Email,
                    PasswordHash = passwordHash,
                    RoleId = roleId,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    DateOfBirth = record.DateOfBirth,
                    PhoneNumber = record.PhoneNumber,
                    Address = record.Address,
                    City = record.City,
                    AcademicProgram = record.AcademicProgram,
                    EnrollmentDate = DateTime.Now,
                    // Tạo sẵn học bạ như luồng đăng ký thủ công, để GPA/tín chỉ cập nhật được về sau.
                    AcademicRecord = new AcademicRecord
                    {
                        GPA = 0,
                        TotalCreditsCompleted = 0,
                        YearStarted = currentYear
                    }
                });
                result.Imported++;

                if (batch.Count >= BatchSize)
                    await FlushBatchAsync(batch, ct);
            }

            // Ghi nốt lô cuối còn dư.
            await FlushBatchAsync(batch, ct);

            stopwatch.Stop();
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        private async Task FlushBatchAsync(List<Student> batch, CancellationToken ct)
        {
            if (batch.Count == 0) return;

            _context.Students.AddRange(batch);
            await _context.SaveChangesAsync(ct);

            // Gỡ các entity vừa ghi khỏi change-tracker: giữ mức RAM phẳng dù file có bao nhiêu dòng.
            _context.ChangeTracker.Clear();
            batch.Clear();
        }
    }
}
