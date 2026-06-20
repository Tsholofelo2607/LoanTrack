namespace LoanTrack.Core.DTOs;

public class OverdueLoanDto
{
    public Guid LoanApplicationId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string LoanProductName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public int OverdueInstallments { get; set; }
    public decimal TotalOverdueAmount { get; set; }
}