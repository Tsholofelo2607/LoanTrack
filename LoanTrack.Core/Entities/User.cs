namespace LoanTrack.Core.Entities;

public class User
{
    public Guid Id { get; set; } //every entity needs a primary key
    public string FullName { get; set; } = string.Empty; //avoiding null references errors
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Applicant, LoanOfficer, Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — EF uses this to build the JOIN
    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}