namespace SIMS.Web.Services.CsvImport
{
    // Kết quả parse MỘT dòng CSV: hoặc ra được đối tượng T, hoặc là một thông báo lỗi.
    // Gói cả hai vào một kiểu để hàm parse không cần ném exception cho mỗi dòng hỏng
    // (file lớn có thể có hàng nghìn dòng lỗi — dùng exception sẽ vừa chậm vừa khó gom).
    public class RowParseResult<T>
    {
        public T? Value { get; }
        public string? Error { get; }
        public bool Success => Error == null;

        private RowParseResult(T? value, string? error)
        {
            Value = value;
            Error = error;
        }

        public static RowParseResult<T> Ok(T value) => new(value, null);
        public static RowParseResult<T> Fail(string error) => new(default, error);
    }

    // Abstraction cho việc chuyển 1 dòng CSV -> 1 bản ghi đã kiểm tra hợp lệ.
    //
    // - DIP: dịch vụ import phụ thuộc vào interface này, không biết chi tiết mapping từng loại dữ liệu.
    // - OCP: muốn import thêm loại khác (Course, Enrollment...) chỉ cần thêm một parser mới,
    //        không phải sửa dịch vụ import đang chạy.
    // - SRP: parser chỉ lo đọc/validate một dòng; việc ghi DB, chống trùng, băm mật khẩu
    //        là trách nhiệm của tầng import.
    public interface ICsvRowParser<T>
    {
        // Thứ tự cột kỳ vọng (dùng để validate header và sinh file mẫu).
        string[] ExpectedHeader { get; }

        RowParseResult<T> Parse(string[] fields, int lineNumber);
    }
}
