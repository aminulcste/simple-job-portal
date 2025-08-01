namespace OnlineJobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }

        public string? Title { get; set; }           // ✅ Nullable
        public string? Description { get; set; }     // ✅ Nullable
        public string? Location { get; set; }        // ✅ Nullable

        public DateTime PostedDate { get; set; }

        public string? EmployerId { get; set; }      // ✅ Nullable
        public ApplicationUser? Employer { get; set; } // ✅ Nullable navigation property
    }
}
