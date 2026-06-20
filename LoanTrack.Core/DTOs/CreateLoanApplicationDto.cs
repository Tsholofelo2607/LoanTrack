namespace LoanTrack.Core.DTOs;

public class CreateLoanApplicationDto
{
    public Guid LoanProductId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int TermMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
}