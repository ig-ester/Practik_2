using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktik.Infrastructure.Data;

namespace Praktik.Controllers;

[Route("api/batches")]
[ApiController]
public class BatchesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BatchesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-product")]
    public IActionResult GetByProduct([FromQuery] int productId)
    {
        try
        {
            var batches = _context.Batches
                .Where(b => b.ProductId == productId)
                .Select(b => new
                {
                    id = b.Id,
                    seriesNumber = b.SeriesNumber,
                    expirationDate = b.ExpirationDate
                })
                .ToList();

            return Ok(batches);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock([FromQuery] int warehouseId, [FromQuery] string date)
    {
        try
        {
            var stockData = await _context.Stock
                .Include(s => s.Batch).ThenInclude(b => b.Product)
                .Where(s => s.WarehouseId == warehouseId && s.Quantity > 0)
                .Select(s => new
                {
                    productId = s.Batch.ProductId,
                    productName = s.Batch.Product.Name,
                    batchId = s.Batch.Id,
                    seriesNumber = s.Batch.SeriesNumber,
                    quantity = s.Quantity
                })
                .ToListAsync();

            return Ok(stockData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}