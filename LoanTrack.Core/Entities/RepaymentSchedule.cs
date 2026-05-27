namespace LoanTrack.Core.Entities;

public class RepaymentSchedule
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }

    public int InstallmentNumber { get; set; }   // 1, 2, 3... up to TermMonths
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsPaid { get; set; } = false;

    public LoanApplication LoanApplication { get; set; } = null!;
}