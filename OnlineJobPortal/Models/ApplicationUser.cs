using Microsoft.AspNetCore.Identity;

namespace OnlineJobPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
           public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // This is for easier role assignment if you want
    // ✅ Nullable
        public string? CVPath { get; set; } // For jobseekers only
    }

}
