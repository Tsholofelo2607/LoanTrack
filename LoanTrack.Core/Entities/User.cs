using Microsoft.AspNetCore.Identity;

namespace LoanTrack.Core.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// Refresh token fields
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    
    // Navigation property
    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}