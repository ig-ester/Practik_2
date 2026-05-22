using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktik.Infrastructure.Data;

namespace Praktik.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            isPoison = product.IsPoison,
            isZnvlp = product.IsZnvlp,
            isPrescriptionOnly = product.IsPrescriptionOnly,
            isMarked = product.IsMarked,
            markingCategory = product.MarkingCategory
        });
    }
}
