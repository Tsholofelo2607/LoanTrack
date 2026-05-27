namespace LoanTrack.Core.Entities;

public class LoanProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;        // e.g. "Personal Loan"
    public decimal InterestRate { get; set; }                // e.g. 15.5 (percent)
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}