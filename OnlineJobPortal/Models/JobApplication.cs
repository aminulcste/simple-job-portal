namespace OnlineJobPortal.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job? Job { get; set; }  // ✅ Nullable navigation property

        public string? JobSeekerId { get; set; }      // ✅ Nullable string
        public ApplicationUser? JobSeeker { get; set; } // ✅ Nullable navigation property

        public string? CVPath { get; set; }

        public DateTime AppliedDate { get; set; }

        public string? ResumeUrl { get; set; }


        public string Status { get; set; } = "Pending"; // ✅ Default value to avoid warning
    }
}
