using LoanTrack.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoanProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoanProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/loanproducts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.LoanProducts
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.InterestRate,
                p.MinTermMonths,
                p.MaxTermMonths,
                p.MinAmount,
                p.MaxAmount
            })
            .ToListAsync();

        return Ok(products);
    }
}