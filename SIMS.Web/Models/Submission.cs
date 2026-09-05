namespace SIMS.Web.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public string FileName { get; set; } = string.Empty;      // tên file gốc
        public string FilePath { get; set; } = string.Empty;      // đường dẫn lưu trên server
        public DateTime SubmittedAt { get; set; }
        public string Grade { get; set; } = string.Empty;         // A/B/C/D/F
        public string? Feedback { get; set; }                     // nhận xét của giáo viên
        public bool IsGraded { get; set; } = false;
    }
}