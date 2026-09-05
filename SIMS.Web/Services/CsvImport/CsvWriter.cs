using System.Text;

namespace SIMS.Web.Services.CsvImport
{
    // Ghi các trường thành một dòng CSV hợp lệ, đảo ngược với CsvLineSplitter.
    // Bao ô trong dấu nháy kép khi ô có dấu phẩy, nháy kép hoặc xuống dòng;
    // nháy kép bên trong được nhân đôi theo chuẩn CSV.
    public static class CsvWriter
    {
        public static string BuildLine(params object?[] fields)
            => string.Join(",", fields.Select(f => Escape(f?.ToString() ?? string.Empty)));

        private static string Escape(string value)
        {
            bool mustQuote = value.Contains(',') || value.Contains('"')
                             || value.Contains('\n') || value.Contains('\r');
            if (!mustQuote) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
