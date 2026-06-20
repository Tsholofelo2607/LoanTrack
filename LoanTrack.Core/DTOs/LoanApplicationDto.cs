namespace LoanTrack.Core.DTOs;

public class LoanApplicationDto
{
    public Guid Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string LoanProductName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public int TermMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}