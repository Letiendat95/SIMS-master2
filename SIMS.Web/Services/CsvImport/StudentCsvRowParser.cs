namespace SIMS.Web.Services.CsvImport
{
    // Chuyển một dòng CSV -> StudentCsvRecord, đồng thời kiểm tra định dạng từng ô.
    // Chỉ lo mức "dòng" (bắt buộc, đúng kiểu). Việc chống trùng username là mức "toàn file"
    // nên để tầng import lo, không thuộc trách nhiệm của parser.
    public class StudentCsvRowParser : ICsvRowParser<StudentCsvRecord>
    {
        // Mật khẩu mặc định khi cột Password để trống — cho phép import hàng loạt nhanh
        // mà không cần soạn mật khẩu cho từng sinh viên. Admin nên yêu cầu đổi ở lần đăng nhập đầu.
        public const string DefaultPassword = "Sims@12345";

        public string[] ExpectedHeader { get; } =
        {
            "FirstName", "LastName", "Username", "Email", "Password",
            "DateOfBirth", "PhoneNumber", "Address", "City", "AcademicProgram"
        };

        // NOTE_VALIDATE_ERROR: Nơi quyết định một dòng ĐÚNG hay SAI và SAI Ở ĐÂU.
        // Kiểm tra lần lượt: đủ số cột? thiếu Họ/Tên/Username? email có '@'? ngày sinh đọc được?
        // Mỗi lỗi trả về một câu mô tả cụ thể (ví dụ "Email không hợp lệ: '...'") để hiển thị cho admin.
        public RowParseResult<StudentCsvRecord> Parse(string[] fields, int lineNumber)
        {
            if (fields.Length < ExpectedHeader.Length)
                return RowParseResult<StudentCsvRecord>.Fail(
                    $"Cần {ExpectedHeader.Length} cột nhưng chỉ có {fields.Length}.");

            string Get(int i) => fields[i].Trim();

            var firstName = Get(0);
            var lastName = Get(1);
            var username = Get(2);
            var email = Get(3);
            var password = Get(4);

            if (string.IsNullOrWhiteSpace(firstName))
                return RowParseResult<StudentCsvRecord>.Fail("Thiếu FirstName.");
            if (string.IsNullOrWhiteSpace(lastName))
                return RowParseResult<StudentCsvRecord>.Fail("Thiếu LastName.");
            if (string.IsNullOrWhiteSpace(username))
                return RowParseResult<StudentCsvRecord>.Fail("Thiếu Username.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return RowParseResult<StudentCsvRecord>.Fail($"Email không hợp lệ: '{email}'.");

            DateTime? dob = null;
            var dobRaw = Get(5);
            if (!string.IsNullOrWhiteSpace(dobRaw))
            {
                if (!DateTime.TryParse(dobRaw, out var parsed))
                    return RowParseResult<StudentCsvRecord>.Fail(
                        $"DateOfBirth không đọc được: '{dobRaw}' (nên dùng dạng yyyy-MM-dd).");
                dob = parsed;
            }

            var record = new StudentCsvRecord
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                Email = email,
                Password = string.IsNullOrWhiteSpace(password) ? DefaultPassword : password,
                DateOfBirth = dob,
                PhoneNumber = Get(6),
                Address = Get(7),
                City = Get(8),
                AcademicProgram = Get(9)
            };

            return RowParseResult<StudentCsvRecord>.Ok(record);
        }
    }
}
