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
public class LoanApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoanApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/loanapplications
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanApplicationDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var product = await _context.LoanProducts.FindAsync(dto.LoanProductId);
        if (product == null || !product.IsActive)
            return BadRequest("Invalid or inactive loan product.");

        if (dto.RequestedAmount < product.MinAmount || dto.RequestedAmount > product.MaxAmount)
            return BadRequest($"Requested amount must be between {product.MinAmount} and {product.MaxAmount}.");

        if (dto.TermMonths < product.MinTermMonths || dto.TermMonths > product.MaxTermMonths)
            return BadRequest($"Term must be between {product.MinTermMonths} and {product.MaxTermMonths} months.");

        var application = new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantId = userId,
            LoanProductId = dto.LoanProductId,
            RequestedAmount = dto.RequestedAmount,
            TermMonths = dto.TermMonths,
            Purpose = dto.Purpose,
            Status = LoanStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        _context.LoanApplications.Add(application);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, MapToDto(application, product));
    }

    // GET: api/loanapplications
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isOfficerOrAdmin = User.IsInRole("LoanOfficer") || User.IsInRole("Admin");

        var query = _context.LoanApplications
            .Include(a => a.Applicant)
            .Include(a => a.LoanProduct)
            .AsQueryable();

        if (!isOfficerOrAdmin)
            query = query.Where(a => a.ApplicantId == userId);

        var applications = await query
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        return Ok(applications.Select(a => MapToDto(a, a.LoanProduct)));
    }

    // GET: api/loanapplications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isOfficerOrAdmin = User.IsInRole("LoanOfficer") || User.IsInRole("Admin");

        var application = await _context.LoanApplications
            .Include(a => a.Applicant)
            .Include(a => a.LoanProduct)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound();

        if (!isOfficerOrAdmin && application.ApplicantId != userId)
            return Forbid();

        return Ok(MapToDto(application, application.LoanProduct));
    }

    // PUT: api/loanapplications/{id}/review
    [HttpPut("{id}/review")]
    [Authorize(Roles = "LoanOfficer,Admin")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewLoanApplicationDto dto)
    {
        var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var application = await _context.LoanApplications
            .Include(a => a.LoanProduct)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound();

        if (application.Status != LoanStatus.Submitted && application.Status != LoanStatus.UnderReview)
            return BadRequest("This application has already been reviewed.");

        application.Status = dto.Approve ? LoanStatus.Approved : LoanStatus.Rejected;
        application.ReviewNotes = dto.Notes;
        application.ReviewedByUserId = officerId;
        application.ReviewedAt = DateTime.UtcNow;

        // Auto-generate repayment schedule on approval
        if (dto.Approve)
        {
            var schedules = GenerateRepaymentSchedule(application);
            _context.RepaymentSchedules.AddRange(schedules);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = dto.Approve ? "Loan approved." : "Loan rejected.", status = application.Status.ToString() });
    }

    private static List<RepaymentSchedule> GenerateRepaymentSchedule(LoanApplication application)
    {
        var schedules = new List<RepaymentSchedule>();
        var monthlyRate = application.LoanProduct.InterestRate / 100 / 12;
        var principal = application.RequestedAmount;
        var term = application.TermMonths;

        // Standard amortisation formula: M = P * [r(1+r)^n] / [(1+r)^n - 1]
        var monthlyPayment = principal *
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), term)) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), term) - 1);

        for (int i = 1; i <= term; i++)
        {
            schedules.Add(new RepaymentSchedule
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = application.Id,
                InstallmentNumber = i,
                AmountDue = Math.Round(monthlyPayment, 2),
                AmountPaid = 0,
                DueDate = DateTime.UtcNow.AddMonths(i),
                IsPaid = false
            });
        }

        return schedules;
    }

    private static LoanApplicationDto MapToDto(LoanApplication a, LoanProduct product)
    {
        return new LoanApplicationDto
        {
            Id = a.Id,
            ApplicantName = a.Applicant?.FullName ?? string.Empty,
            LoanProductName = product.Name,
            RequestedAmount = a.RequestedAmount,
            TermMonths = a.TermMonths,
            Purpose = a.Purpose,
            Status = a.Status.ToString(),
            ReviewNotes = a.ReviewNotes,
            SubmittedAt = a.SubmittedAt,
            ReviewedAt = a.ReviewedAt
        };
    }
}