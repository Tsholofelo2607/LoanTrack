namespace LoanTrack.Core.DTOs;

public class RepaymentScheduleDto
{
    public int InstallmentNumber { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
}