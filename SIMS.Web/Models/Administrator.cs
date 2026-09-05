namespace SIMS.Web.Models
{
    public class Administrator : User
    {
        public string AdminId { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}
