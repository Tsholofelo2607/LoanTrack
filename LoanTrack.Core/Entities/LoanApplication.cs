namespace LoanTrack.Core.Entities;

public class LoanApplication
{
    public Guid Id { get; set; }

    // Foreign keys
    public Guid ApplicantId { get; set; }
    public Guid LoanProductId { get; set; }
    public Guid? ReviewedByUserId { get; set; }  // nullable — not assigned until reviewed

    // Loan details
    public decimal RequestedAmount { get; set; }
    public int TermMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;

    // Workflow
    public LoanStatus Status { get; set; } = LoanStatus.Submitted;
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Timestamps
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Applicant { get; set; } = null!;
    public LoanProduct LoanProduct { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
    public ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
}

public enum LoanStatus
{
    Submitted = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Disbursed = 4,
    Repaid = 5
}