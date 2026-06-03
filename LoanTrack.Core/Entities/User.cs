using Microsoft.AspNetCore.Identity;

namespace LoanTrack.Core.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}