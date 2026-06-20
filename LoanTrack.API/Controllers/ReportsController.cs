using System.Security.Claims;
using LoanTrack.Core.DTOs;
using LoanTrack.Core.Entities;
using LoanTrack.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/reports/my-loans?status=Approved&page=1&pageSize=10
    // Applicant sees their own loans with optional status filter and pagination
    [HttpGet("my-loans")]
    public async Task<IActionResult> MyLoans(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = _context.LoanApplications
            .Include(a => a.LoanProduct)
            .Where(a => a.ApplicantId == userId)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LoanStatus>(status, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);

        var totalCount = await query.CountAsync();

        var loans = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                LoanProduct = a.LoanProduct.Name,
                a.RequestedAmount,
                a.TermMonths,
                a.Purpose,
                Status = a.Status.ToString(),
                a.SubmittedAt
            })
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            data = loans
        });
    }

    // GET: api/reports/overdue-loans
    // Officers and Admins only — loans with unpaid overdue installments
    [HttpGet("overdue-loans")]
    [Authorize(Roles = "LoanOfficer,Admin")]
    public async Task<IActionResult> OverdueLoans()
    {
        var today = DateTime.UtcNow;

        var overdueLoans = await _context.RepaymentSchedules
            .Include(r => r.LoanApplication)
                .ThenInclude(a => a.Applicant)
            .Include(r => r.LoanApplication)
                .ThenInclude(a => a.LoanProduct)
            .Where(r => !r.IsPaid && r.DueDate < today)
            .GroupBy(r => r.LoanApplicationId)
            .Select(g => new OverdueLoanDto
            {
                LoanApplicationId = g.Key,
                ApplicantName = g.First().LoanApplication.Applicant.FullName,
                LoanProductName = g.First().LoanApplication.LoanProduct.Name,
                RequestedAmount = g.First().LoanApplication.RequestedAmount,
                OverdueInstallments = g.Count(),
                TotalOverdueAmount = g.Sum(r => r.AmountDue - r.AmountPaid)
            })
            .OrderByDescending(o => o.TotalOverdueAmount)
            .ToListAsync();

        return Ok(overdueLoans);
    }

    // GET: api/reports/monthly-disbursements
    // Admins only — total loans approved per month
    [HttpGet("monthly-disbursements")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MonthlyDisbursements()
    {
        var disbursements = await _context.LoanApplications
            .Where(a => a.Status == LoanStatus.Approved || a.Status == LoanStatus.Disbursed)
            .GroupBy(a => new { a.ReviewedAt!.Value.Year, a.ReviewedAt.Value.Month })
            .Select(g => new MonthlyDisbursementDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalLoansApproved = g.Count(),
                TotalAmountDisbursed = g.Sum(a => a.RequestedAmount)
            })
            .OrderByDescending(d => d.Year)
            .ThenByDescending(d => d.Month)
            .ToListAsync();

        return Ok(disbursements);
    }

    // GET: api/reports/repayment-schedule/{loanId}
    // Shows installments with running remaining balance
    [HttpGet("repayment-schedule/{loanId}")]
    public async Task<IActionResult> RepaymentSchedule(Guid loanId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isOfficerOrAdmin = User.IsInRole("LoanOfficer") || User.IsInRole("Admin");

        var application = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.Id == loanId);

        if (application == null)
            return NotFound();

        // Applicants can only see their own schedules
        if (!isOfficerOrAdmin && application.ApplicantId != userId)
            return Forbid();

        var schedules = await _context.RepaymentSchedules
            .Where(r => r.LoanApplicationId == loanId)
            .OrderBy(r => r.InstallmentNumber)
            .ToListAsync();

        // Calculate running remaining balance in memory
        // This is the window function equivalent in C# — 
        // total amount due minus everything paid so far
        var totalDue = schedules.Sum(r => r.AmountDue);
        var runningPaid = 0m;

        var result = schedules.Select(r =>
        {
            runningPaid += r.AmountPaid;
            return new RepaymentScheduleDto
            {
                InstallmentNumber = r.InstallmentNumber,
                AmountDue = r.AmountDue,
                AmountPaid = r.AmountPaid,
                RemainingBalance = totalDue - runningPaid,
                DueDate = r.DueDate,
                IsPaid = r.IsPaid
            };
        }).ToList();

        return Ok(new
        {
            loanId,
            totalAmount = totalDue,
            totalPaid = runningPaid,
            remainingBalance = totalDue - runningPaid,
            schedule = result
        });
    }
}