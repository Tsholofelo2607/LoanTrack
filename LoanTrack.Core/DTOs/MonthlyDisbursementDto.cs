namespace LoanTrack.Core.DTOs;

public class MonthlyDisbursementDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalLoansApproved { get; set; }
    public decimal TotalAmountDisbursed { get; set; }
}