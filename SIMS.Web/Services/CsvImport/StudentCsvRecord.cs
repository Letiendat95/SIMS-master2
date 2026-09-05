namespace SIMS.Web.Services.CsvImport
{
    // Bản ghi "thô" đã tách từ một dòng CSV và đã kiểm tra định dạng cơ bản.
    // Chưa phải entity Student: chưa băm mật khẩu, chưa gán RoleId, chưa chống trùng —
    // những việc đó do StudentCsvImportService xử lý (tách bạch trách nhiệm).
    public class StudentCsvRecord
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AcademicProgram { get; set; } = string.Empty;
    }
}
