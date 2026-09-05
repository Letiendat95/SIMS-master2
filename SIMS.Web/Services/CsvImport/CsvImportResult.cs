namespace SIMS.Web.Services.CsvImport
{
    public record CsvImportError(int LineNumber, string Reason);

    // Tổng kết một lần import để hiển thị lại cho admin.
    public class CsvImportResult
    {
        // Giới hạn số lỗi GIỮ LẠI trong bộ nhớ. File lớn có thể sinh hàng chục nghìn lỗi;
        // giữ hết sẽ ngốn RAM và trang kết quả không đọc nổi. Vẫn ĐẾM đủ tổng số lỗi
        // qua thuộc tính Failed, chỉ danh sách chi tiết là bị cắt bớt (có báo rõ, không cắt ngầm).
        private const int MaxErrorsTracked = 200;

        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Failed => TotalRows - Imported;

        public List<CsvImportError> Errors { get; } = new();
        public int ErrorsNotShown { get; private set; }
        public long ElapsedMs { get; set; }

        public void AddError(int lineNumber, string reason)
        {
            if (Errors.Count < MaxErrorsTracked)
                Errors.Add(new CsvImportError(lineNumber, reason));
            else
                ErrorsNotShown++;
        }
    }
}
